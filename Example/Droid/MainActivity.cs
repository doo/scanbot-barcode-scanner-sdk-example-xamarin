using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Provider;
using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.Barcode_scanner;
using IO.Scanbot.Sdk.UI.Barcode_scanner.View.Barcode;
using IO.Scanbot.Sdk.UI.View.Barcode;
using IO.Scanbot.Sdk.UI.View.Barcode.Configuration;
namespace BarcodeScannerExample.Droid
{
    [Activity(MainLauncher = true, Theme = "@style/AppTheme", Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        View WarningView => FindViewById<View>(Resource.Id.warning_view);

        ScanbotBarcodeScannerSDK SDK => new ScanbotBarcodeScannerSDK(this);
        bool IsLicenseValid
        {
            get
            {
                return SDK.LicenseInfo.Status == IO.Scanbot.Sap.Status.StatusOkay
                    || SDK.LicenseInfo.Status == IO.Scanbot.Sap.Status.StatusTrial;
            }
        }

        const int BARCODE_DEFAULT_UI_REQUEST_CODE = 910;
        const int IMPORT_IMAGE_REQUEST_CODE = 911;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            FindViewById<TextView>(Resource.Id.qr_demo).Click += OnQRClick;
            FindViewById<TextView>(Resource.Id.rtu_ui).Click += OnRTUUIClick;
            FindViewById<TextView>(Resource.Id.rtu_ui_image).Click += OnRTUUIImageClick;
            FindViewById<TextView>(Resource.Id.rtu_ui_import).Click += OnRTUUIImportClick;
            FindViewById<TextView>(Resource.Id.settings).Click += OnSettingsClick;
        }

        private void OnQRClick(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(QRScanCameraViewActivity));
            StartActivity(intent);
        }

        private void OnRTUUIClick(object sender, EventArgs e)
        {
            StartBarcodeScannerActivity(BarcodeImageGenerationType.None);
        }

        private void OnRTUUIImageClick(object sender, EventArgs e)
        {
            StartBarcodeScannerActivity(BarcodeImageGenerationType.VideoFrame);
        }

        private async void OnRTUUIImportClick(object sender, EventArgs e)
        {
            Bitmap bitmap = await Scanbot.ImagePicker.Droid.ImagePicker.Instance.Pick();

            var result = SDK.BarcodeDetector().DetectFromBitmap(bitmap, 0);

            BarcodeResultBundle.Instance = new BarcodeResultBundle
            {
                ScanningResult = result
            };

            StartActivity(new Intent(this, typeof(BarcodeResultActivity)));
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(BarcodeTypesActivity));
            StartActivity(intent);
        }

        void StartBarcodeScannerActivity(BarcodeImageGenerationType type)
        {
            var configuration = new BarcodeScannerConfiguration();
            var list = BarcodeFormat.Values();
            // The option to set barcode formats filter is missing in Xamarin android
            // It is a known issue that will be fixed in the future
            //configuration.SetBarcodeFormatsFilter(list);
            configuration.SetBarcodeImageGenerationType(type);

            var intent = BarcodeScannerActivity.NewIntent(this, configuration);
            StartActivityForResult(intent, BARCODE_DEFAULT_UI_REQUEST_CODE);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
            {
                return;
            }

            if (!SDK.LicenseInfo.IsValid)
            {
                WarningView.Visibility = ViewStates.Visible;
                var text = "License invalid, unable to perform action";
                Toast.MakeText(this, text, ToastLength.Long);
                return;
            }

            if (requestCode == BARCODE_DEFAULT_UI_REQUEST_CODE)
            {
                var barcode = (BarcodeScanningResult)data.GetParcelableExtra(
                    BaseBarcodeScannerActivity.ScannedBarcodeExtra);
                var imagePath = data.GetStringExtra(
                    BaseBarcodeScannerActivity.ScannedBarcodeImagePathExtra);
                var previewPath = data.GetStringExtra(
                    BaseBarcodeScannerActivity.ScannedBarcodePreviewFramePathExtra);

                BarcodeResultBundle.Instance = new BarcodeResultBundle
                {
                    ScanningResult = barcode,
                    ImagePath = imagePath,
                    PreviewPath = previewPath
                };

                StartActivity(new Intent(this, typeof(BarcodeResultActivity)));
            }
        }

        private Bitmap ProcessGalleryResult(Intent data)
        {
            var imageUri = data.Data;
            Bitmap bitmap = null;
            if (imageUri != null)
            {
                try
                {
                    bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, imageUri);
                }
                catch (Exception)
                {
                }
            }
            return bitmap;
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (IsLicenseValid)
            {
                WarningView.Visibility = ViewStates.Gone;
            }
            else
            {
                WarningView.Visibility = ViewStates.Visible;
            }

        }
    }
}

