﻿using System;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using IO.Scanbot.Sdk.Barcode;
using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.Barcode_scanner;
using IO.Scanbot.Sdk.Camera;

namespace BarcodeScannerExample.Droid
{
    [Activity(Theme = "@style/AppTheme")]
    public class QRScanCameraViewActivity : AppCompatActivity, ICameraOpenCallback
    {
        ScanbotCameraView cameraView;

        BarcodeDetectorFrameHandler handler;

        const int REQUEST_PERMISSION_CODE = 200;
        public string[] Permissions
        {
            get => new string[] { Manifest.Permission.Camera };
        }

        bool flashEnabled = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SupportRequestWindowFeature(WindowCompat.FeatureActionBarOverlay);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.qr_camera_view);

            cameraView = FindViewById<ScanbotCameraView>(Resource.Id.camera);

            cameraView.SetCameraOpenCallback(this);

            var SDK = new ScanbotBarcodeScannerSDK(this);
            var detector = SDK.BarcodeDetector();
            detector.SetBarcodeFormatsFilter(BarcodeTypes.Instance.AcceptedTypes);

            handler = BarcodeDetectorFrameHandler.Attach(cameraView, detector);
            handler.SetDetectionInterval(1000);

            var resultHandler = new BarcodeResultDelegate();
            handler.AddResultHandler(resultHandler);
            resultHandler.Success += OnBarcodeResult;

            FindViewById<Button>(Resource.Id.flash).Click += delegate
            {
                flashEnabled = !flashEnabled;
                cameraView.UseFlash(flashEnabled);
            };
        }

        private void OnBarcodeResult(object sender, BarcodeEventArgs e)
        {
            RunOnUiThread(() => {
                // show the first scanned item from the list
                Toast.MakeText(this, e.Result.BarcodeItems[0].BarcodeFormat.ToString() + ": " + e.Result.BarcodeItems[0].Text, ToastLength.Short).Show();
            });
        }

        protected override void OnResume()
        {
            base.OnResume();
            cameraView.OnResume();
            var status = ContextCompat.CheckSelfPermission(this, Permissions[0]);
            if (status != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, Permissions, REQUEST_PERMISSION_CODE);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            cameraView.OnPause();
        }

        public void OnCameraOpened()
        {
            cameraView.PostDelayed(delegate
            {
                cameraView.UseFlash(flashEnabled);
                cameraView.ContinuousFocus();
            }, 300);
        }
    }

    class BarcodeEventArgs : EventArgs
    {
        public BarcodeScanningResult Result { get; private set; }

        public BarcodeEventArgs(Java.Lang.Object value)
        {
            Result = (BarcodeScanningResult)value;
        }
    }

    class BarcodeResultDelegate : BarcodeDetectorFrameHandler.BarcodeDetectorResultHandler
    {
        public EventHandler<BarcodeEventArgs> Success;

        public override bool Handle(FrameHandlerResult p0)
        {
            var success = (FrameHandlerResult.Success)p0;
            if (success != null && success.Value != null)
            {
                Success?.Invoke(this, new BarcodeEventArgs(success.Value));
            }

            return false;
        }
    }

}
