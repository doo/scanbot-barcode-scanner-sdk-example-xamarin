using System.Collections.Generic;
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
using IO.Scanbot.Sdk.Barcode.UI;
using IO.Scanbot.Sdk.Barcode_scanner;
using IO.Scanbot.Sdk.Camera;
using Java.Util;

namespace BarcodeScannerExample.Droid
{
    [Activity (Label = "DemoBarcodeScannerViewActivity", Theme = "@style/AppTheme")]            
    public class DemoBarcodeScannerViewActivity : AppCompatActivity, IBarcodeScannerViewCallback
    {
        BarcodeScannerView barcodeScannerView;
        ImageView resultView;

        bool flashEnabled;
        readonly BarcodeDetectorFrameHandler handler;

        const int REQUEST_PERMISSION_CODE = 200;
        public string[] Permissions
        {
            get => new string[] { Manifest.Permission.Camera };
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SupportRequestWindowFeature(WindowCompat.FeatureActionBarOverlay);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.barcode_scanner_view_activity);

            barcodeScannerView = FindViewById<BarcodeScannerView>(Resource.Id.camera);
            BarcodeScannerViewWrapper.InitCamera(barcodeScannerView);

            resultView = FindViewById<ImageView>(Resource.Id.result);

            FindViewById<Button>(Resource.Id.flash).Click += delegate
            {
                flashEnabled = !flashEnabled;
                barcodeScannerView.ViewController.UseFlash(flashEnabled);
            };

            var SDK = new ScanbotBarcodeScannerSDK(this);

            var resultDelegate = new BarcodeResultDelegate();

            // UNCOMMENT THIS TO ENABLE AUTOMATIC BARCODE SELECTION:
            // resultDelegate.Success += OnBarcodeResult;

            var barcodeDetector = SDK.CreateBarcodeDetector();
            BarcodeScannerViewWrapper.InitDetectionBehavior(barcodeScannerView, barcodeDetector, resultDelegate, this);

            // UNCOMMENT THIS TO ENABLE AUTOSNAPPING:
            //barcodeScannerView.ViewController.AutoSnappingEnabled = true;
            //barcodeScannerView.ViewController.SetAutoSnappingSensitivity(1f);

            barcodeScannerView.SelectionOverlayController.SetEnabled(true);
            barcodeScannerView.FinderViewController.SetFinderEnabled(false);
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
            var status = ContextCompat.CheckSelfPermission(this, Permissions[0]);
            if (status != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, Permissions, REQUEST_PERMISSION_CODE);
            }

            barcodeScannerView.ViewController.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            barcodeScannerView.ViewController.OnPause();
        }

        public void OnSelectionOverlayBarcodeClicked(BarcodeItem barcodeItem)
        {
            var items = new List<BarcodeItem>
            {
                barcodeItem
            };

            var args = new BarcodeEventArgs(new BarcodeScanningResult(items, new Date().Time));

            OnBarcodeResult(this, args);
        }

        public void OnCameraOpen()
        {
            barcodeScannerView.PostDelayed(delegate
            {
                barcodeScannerView.ViewController.UseFlash(flashEnabled);
                barcodeScannerView.ViewController.ContinuousFocus();
            }, 300);
        }

        public void OnPictureTaken(byte[] image, CaptureInfo captureInfo)
        {
            var orientation = captureInfo.ImageOrientation;

            var bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);

            if (bitmap == null)
            {
                return;
            }

            var matrix = new Matrix();
            matrix.SetRotate(orientation, bitmap.Width / 2, bitmap.Height / 2);

            var result = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, false);

            resultView.Post(delegate
            {
                resultView.SetImageBitmap(result);
                barcodeScannerView.ViewController.ContinuousFocus();
                barcodeScannerView.ViewController.StartPreview();
            });
        }
    }
}

