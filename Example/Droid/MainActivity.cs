﻿using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.Barcode_scanner;
using IO.Scanbot.Sdk.UI.Barcode_scanner.View.Barcode;
using IO.Scanbot.Sdk.UI.Barcode_scanner.View.Barcode.Batch;
using IO.Scanbot.Sdk.UI.View.Barcode.Batch.Configuration;
using IO.Scanbot.Sdk.UI.View.Barcode.Configuration;
using IO.Scanbot.Sdk.UI.View.Base;

namespace BarcodeScannerExample.Droid
{
    [Activity(MainLauncher = true, Theme = "@style/AppTheme", Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        View WarningView => FindViewById<View>(Resource.Id.warning_view);

        public static ScanbotBarcodeScannerSDK SDK;
        
        const int BARCODE_DEFAULT_UI_REQUEST_CODE = 910;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SDK = new ScanbotBarcodeScannerSDK(this);

            SetContentView(Resource.Layout.activity_main);

            FindViewById<TextView>(Resource.Id.barcode_scanner_view_demo).Click += OnBarcodeScannerViewClick;
            FindViewById<TextView>(Resource.Id.barcode_camerax_demo).Click += OnBarcodeCameraXDemoClick;
            FindViewById<TextView>(Resource.Id.rtu_ui).Click += OnRTUUIClick;
            FindViewById<TextView>(Resource.Id.rtu_ui_image).Click += OnRTUUIImageClick;
            FindViewById<TextView>(Resource.Id.rtu_ui_aroverlay).Click += OnRTUUIAROverlayClick;
            FindViewById<TextView>(Resource.Id.batch_rtu_ui).Click += OnBatchRTUUIClick;
            FindViewById<TextView>(Resource.Id.rtu_ui_import).Click += OnImportClick;
            FindViewById<TextView>(Resource.Id.settings).Click += OnSettingsClick;
            FindViewById<TextView>(Resource.Id.clear_storage).Click += OnClearStorageClick;
            FindViewById<TextView>(Resource.Id.license_info).Click += OnLicenseInfoClick;
        }

        private void OnBarcodeScannerViewClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            var intent = new Intent(this, typeof(DemoBarcodeScannerViewActivity));
            StartActivity(intent);
        }

        private void OnBarcodeCameraXDemoClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            var intent = new Intent(this, typeof(DemoBarcodeCameraXViewActivity));
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

        private void OnRTUUIAROverlayClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            StartBarcodeScannerActivityWithAROverlay();
        }

        private void OnBatchRTUUIClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            StartBatchBarcodeScannerActivity();
        }

        private async void OnImportClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this, SDK))
            {
                return;
            }
            Bitmap bitmap = await Scanbot.ImagePicker.Droid.ImagePicker.Instance.Pick();

            if (bitmap == null)
            {
                return;
            }

            var result = SDK.CreateBarcodeDetector().DetectFromBitmap(bitmap, 0);

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
            SDK.CreateBarcodeFileStorage().CleanupBarcodeImagesDirectory();
            Alert.Toast(this, "Cleared image storage");
        }

        private void OnLicenseInfoClick(object sender, EventArgs e)
        {
            var status = SDK.LicenseInfo.Status;
            var date = SDK.LicenseInfo.ExpirationDate;
            var validity = SDK.LicenseInfo.IsValid ? "The license is valid." : "The license is NOT valid";
            var message = validity + $"\n\n- Status: {status}";
            if (date != null)
            {
                message += $"\n- Valid until: {date}";
            }

            Alert.ShowInfoDialog(this, "License Info", message);
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

        void StartBarcodeScannerActivityWithAROverlay()
        {
            var configuration = new BarcodeScannerConfiguration();
            var list = BarcodeTypes.Instance.AcceptedTypes;
            configuration.SetBarcodeFormatsFilter(list);
            configuration.SetSelectionOverlayConfiguration(new IO.Scanbot.Sdk.UI.View.Barcode.SelectionOverlayConfiguration(
                overlayEnabled: true,
                automaticSelectionEnabled: false,
                textFormat: IO.Scanbot.Sdk.Barcode.UI.BarcodeOverlayTextFormat.Code,
                polygonColor: Color.Yellow,
                textColor: Color.Yellow,
                textContainerColor: Color.Black)
            );

            var intent = BarcodeScannerActivity.NewIntent(this, configuration);
            StartActivityForResult(intent, BARCODE_DEFAULT_UI_REQUEST_CODE);
        }

        void StartBatchBarcodeScannerActivity()
        {
            var configuration = new BatchBarcodeScannerConfiguration();
            var list = BarcodeTypes.Instance.AcceptedTypes;
            configuration.SetBarcodeFormatsFilter(list);
            var intent = BatchBarcodeScannerActivity.NewIntent(this, configuration);
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
                    RtuConstants.ExtraKeyRtuResult);
                var imagePath = data.GetStringExtra(
                    BarcodeScannerActivity.ScannedBarcodeImagePathExtra);
                var previewPath = data.GetStringExtra(
                    BarcodeScannerActivity.ScannedBarcodePreviewFramePathExtra);

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
            UpdateLicenseStatusWarning();
        }

        private void UpdateLicenseStatusWarning()
        {
            if (SDK.LicenseInfo.Status == IO.Scanbot.Sap.Status.StatusTrial)
            {
                WarningView.Visibility = ViewStates.Visible;
            }
            else
            {
                WarningView.Visibility = ViewStates.Gone;
            }
        }
    }
}

