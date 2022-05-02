using System;
using Android;
using Android.App;
using Android.Content;
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
using IO.Scanbot.Sdk.UI.Camera;

namespace BarcodeScannerExample.Droid
{
    [Activity(Theme = "@style/AppTheme")]
    public class QRScanCameraViewActivity : AppCompatActivity, ICameraOpenCallback
    {
        ScanbotCameraXView cameraView;
        CameraModule cameraModule = CameraModule.Back;

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

            cameraView = FindViewById<ScanbotCameraXView>(Resource.Id.camera);
            cameraView.SetCameraModule(cameraModule);
            cameraView.SetPreviewMode(CameraPreviewMode.FitIn);
            cameraView.SetCameraOpenCallback(this);

            var SDK = new ScanbotBarcodeScannerSDK(this);
            var detector = SDK.CreateBarcodeDetector();
            
            detector.ModifyConfig(new Function1Impl<BarcodeScannerConfigBuilder>((response) => {
                response.SetBarcodeFormats(BarcodeTypes.Instance.AcceptedTypes);
            }));
            
            handler = BarcodeDetectorFrameHandler.Attach(cameraView, detector);
            handler.SetDetectionInterval(500);

            var resultHandler = new BarcodeResultDelegate();
            handler.AddResultHandler(resultHandler);
            resultHandler.Success += OnBarcodeResult;

            FindViewById<Button>(Resource.Id.flash).Click += delegate
            {
                flashEnabled = !flashEnabled;
                cameraView.UseFlash(flashEnabled);
            };

            FindViewById<Button>(Resource.Id.cam_switch).Click += delegate
            {
                cameraModule = cameraModule == CameraModule.Back ? CameraModule.FrontMirrored : CameraModule.Back;
                cameraView.SetCameraModule(cameraModule);
                cameraView.RestartPreview();
            };

            var finderOverlayView = FindViewById<FinderOverlayView>(Resource.Id.my_finder_overlay);
            finderOverlayView.RequiredAspectRatios = new[] { new FinderAspectRatio(300, 150) };
        }

        private void OnBarcodeResult(object sender, BarcodeEventArgs e)
        {
            handler.Enabled = false; // stop detection
            BarcodeResultBundle.Instance = new BarcodeResultBundle(e.Result);
            StartActivity(new Intent(this, typeof(BarcodeResultActivity)));
            Finish();
        }

        protected override void OnResume()
        {
            base.OnResume();

            var status = ContextCompat.CheckSelfPermission(this, Permissions[0]);
            if (status != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, Permissions, REQUEST_PERMISSION_CODE);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
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
                return true;
            }

            return false;
        }
    }

}
