using System;
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
        ImageView resultView;

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
            resultView = FindViewById<ImageView>(Resource.Id.result);

            cameraView.SetCameraOpenCallback(this);

            var SDK = new ScanbotBarcodeScannerSDK(this);
            var detector = SDK.BarcodeDetector();
            
            detector.ModifyConfig(new Function1Impl<BarcodeScannerConfigBuilder>((response) => {
                response.SetSaveCameraPreviewFrame(true);
                response.SetBarcodeFormats(BarcodeTypes.Instance.AcceptedTypes);
            }));

            handler = BarcodeDetectorFrameHandler.Attach(cameraView, detector);
            handler.SetDetectionInterval(1000);

            var resultHandler = new BarcodeResultDelegate();
            handler.AddResultHandler(resultHandler);
            resultHandler.Success += OnBarcodeResult;

            var snappingcontroller = BarcodeAutoSnappingController.Attach(cameraView, handler);
            snappingcontroller.SetSensitivity(1f);

            var pictureDelegate = new PictureResultDelegate();
            cameraView.AddPictureCallback(pictureDelegate);
            pictureDelegate.PictureTaken += OnPictureTaken;

            FindViewById<Button>(Resource.Id.flash).Click += delegate
            {
                flashEnabled = !flashEnabled;
                cameraView.UseFlash(flashEnabled);
            };
        }

        private void OnBarcodeResult(object sender, BarcodeEventArgs e)
        {
            BarcodeResultBundle.Instance = new BarcodeResultBundle(e.Result);
            StartActivity(new Intent(this, typeof(BarcodeResultActivity)));
            Finish();
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

        public void OnPictureTaken(object sender, PictureTakenEventArgs e)
        {
            var image = e.Image;
            var orientation = e.Orientation;

            var bitmap = BitmapFactory.DecodeByteArray(image, 0, orientation);

            var matrix = new Matrix();
            matrix.SetRotate(orientation, bitmap.Width / 2, bitmap.Height / 2);

            var result = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, false);

            resultView.Post(delegate
            {
                resultView.SetImageBitmap(result);
                cameraView.ContinuousFocus();
                cameraView.StartPreview();
            });
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

    public class PictureTakenEventArgs : EventArgs
    {
        public byte[] Image { get; private set; }
        public int Orientation { get; private set; }

        public PictureTakenEventArgs(byte[] image, int orientation)
        {
            Image = image;
            Orientation = orientation;
        }
    }

    class PictureResultDelegate : PictureCallback
    {
        public EventHandler<PictureTakenEventArgs> PictureTaken;

        public override void OnPictureTaken(byte[] image, int imageOrientation)
        {
            PictureTaken?.Invoke(this, new PictureTakenEventArgs(image, imageOrientation));
        }
    }
}
