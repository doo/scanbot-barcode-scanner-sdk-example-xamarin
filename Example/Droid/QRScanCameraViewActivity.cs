using System;
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content.PM;
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

        List<BarcodeScanningResult> scannedBarcodes = new List<BarcodeScanningResult>();

        TextView barcodeCounter;


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
            
            detector.ModifyConfig(new Function1Impl<BarcodeScannerConfigBuilder>((response) => {
                // set accepted barcode types
                response.SetBarcodeFormats(BarcodeTypes.Instance.AcceptedTypes);
            }));
            
            handler = BarcodeDetectorFrameHandler.Attach(cameraView, detector);
            // define a suitable interval for your use case
            handler.SetDetectionInterval(500);

            var resultHandler = new BarcodeResultDelegate();
            handler.AddResultHandler(resultHandler);
            resultHandler.Success += OnBarcodeResult;

            barcodeCounter = FindViewById<TextView>(Resource.Id.barcode_counter);

            FindViewById<Button>(Resource.Id.flash).Click += delegate
            {
                flashEnabled = !flashEnabled;
                cameraView.UseFlash(flashEnabled);
            };

            FindViewById<Button>(Resource.Id.finish_button).Click += delegate
            {
                // close ther scanner (finish Activity).
                // share/pass "scannedBarcodes" to use in your further workflow.
                Finish();
            };
        }

        private void OnBarcodeResult(object sender, BarcodeEventArgs e)
        {
            // collect scanned barcode results in a list
            scannedBarcodes.Add(e.Result);

            // update the barcode counter label
            barcodeCounter.Post(delegate
            {
                barcodeCounter.Text = "Scanned Barcodes: " + scannedBarcodes.Count;
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
                var value = (BarcodeScanningResult)success.Value;
                Success?.Invoke(this, new BarcodeEventArgs(success.Value));
            }

            // return FALSE for continuous barcode scanning!
            return false;
        }
    }

}
