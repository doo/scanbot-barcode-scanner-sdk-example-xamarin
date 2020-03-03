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
using System.Collections.Generic;

namespace BarcodeScannerExample.Droid
{
    [Activity(MainLauncher = true, Theme = "@style/AppTheme", Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        View WarningView => FindViewById<View>(Resource.Id.warning_view);

        ScanbotBarcodeScannerSDK SDK => new ScanbotBarcodeScannerSDK(this);
        
        const int BARCODE_DEFAULT_UI_REQUEST_CODE = 910;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            FindViewById<TextView>(Resource.Id.qr_demo).Click += OnQRClick;
            FindViewById<TextView>(Resource.Id.rtu_ui).Click += OnRTUUIClick;
            FindViewById<TextView>(Resource.Id.rtu_ui_image).Click += OnRTUUIImageClick;
            FindViewById<TextView>(Resource.Id.rtu_ui_import).Click += OnImportClick;
            FindViewById<TextView>(Resource.Id.settings).Click += OnSettingsClick;
            FindViewById<TextView>(Resource.Id.clear_storage).Click += OnClearStorageClick;
            FindViewById<TextView>(Resource.Id.license_info).Click += OnLicenseInfoClick;
        }

        private void OnQRClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            var intent = new Intent(this, typeof(QRScanCameraViewActivity));
            StartActivity(intent);
        }

        private void OnRTUUIClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            StartBarcodeScannerActivity(BarcodeImageGenerationType.None);
        }

        private void OnRTUUIImageClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            StartBarcodeScannerActivity(BarcodeImageGenerationType.VideoFrame);
        }

        private async void OnImportClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            Bitmap bitmap = await Scanbot.ImagePicker.Droid.ImagePicker.Instance.Pick();

            var result = SDK.BarcodeDetector().DetectFromBitmap(bitmap, 0);

            BarcodeResultBundle.Instance = new BarcodeResultBundle
            {
                ScanningResult = result,
                ResultBitmap = bitmap
            };

            StartActivity(new Intent(this, typeof(BarcodeResultActivity)));
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            var intent = new Intent(this, typeof(BarcodeTypesActivity));
            StartActivity(intent);
        }

        private void OnClearStorageClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            SDK.BarcodeFileStorage().CleanupBarcodeImagesDirectory();
            Alert.Toast(this, "Cleared image storage");
        }

        private void OnLicenseInfoClick(object sender, EventArgs e)
        {
            var status = SDK.LicenseInfo.Status;
            var date = SDK.LicenseInfo.ExpirationDate;

            var message = $"License is {status}";
            if (date != null)
            {
                message += $" until {date.ToString()}";
            }

            Alert.Toast(this, message);
        }

        void StartBarcodeScannerActivity(BarcodeImageGenerationType type)
        {
            var configuration = new BarcodeScannerConfiguration();
            var list = BarcodeTypes.Instance.AcceptedTypes;
            configuration.SetBarcodeFormatsFilter(list);
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

        protected override void OnResume()
        {
            base.OnResume();

            if (SDK.LicenseInfo.IsValid)
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

