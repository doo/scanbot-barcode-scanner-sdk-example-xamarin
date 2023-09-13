using IO.Scanbot.Sdk;
using IO.Scanbot.Sdk.Barcode;
using IO.Scanbot.Sdk.Barcode.UI;
using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.Barcode_scanner;
using IO.Scanbot.Sdk.Camera;
using IO.Scanbot.Sdk.UI.Camera;

using Android;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android.Views;
using Android.Widget;

using NativeBarcodeSDKRenderers.Droid.Renderers;
using NativeBarcodeSDKRenderer.Droid.Renderers;

using System.ComponentModel;
using ScanbotBarcodeSDK.Forms.Droid;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using ScanbotBarcodeSDK.Forms;
using System.Collections.Generic;

using static NativeBarcodeSDKRenderer.Views.BarcodeCameraView;
using static IO.Scanbot.Sdk.Barcode.UI.BarcodePolygonsStaticView;

using AndroidColor = Android.Graphics.Color;


/*
    This is the Android Custom Renderer that will provide the actual implementation for BarcodeCameraView.
    We use the 'ExportRenderer' assembly directive to specify that we want to attach AndroidBarcodeCameraRenderer to
    BarcodeCameraView.

    Syntax:

    [assembly: ExportRenderer(typeof([FORMS_VIEW_CLASS]), typeof([CUSTOM_RENDERER_CLASS]))]

    ---
 */
[assembly: ExportRenderer(typeof(NativeBarcodeSDKRenderer.Views.BarcodeCameraView), typeof(AndroidBarcodeCameraRenderer))]
namespace NativeBarcodeSDKRenderers.Droid.Renderers
{
    /*
       By extending 'ViewRenderer' we specify that we want our custom renderer to target 'BarcodeCameraView' and
       override it with our native view, which is a 'FrameLayout' in this case (see layout/barcode_camera_view.xml)
    */
    class AndroidBarcodeCameraRenderer : ViewRenderer<NativeBarcodeSDKRenderer.Views.BarcodeCameraView, FrameLayout>,
        IBarcodeScannerViewCallback
    {
        private bool _isFlashEnabled;
        /// <summary>
        /// Setting to 'true' enables the camera flash, 'false' disables it.
        /// </summary>
        public bool IsFlashEnabled
        {
            get
            {
                return _isFlashEnabled;
            }
            set
            {
                if (cameraView != null)
                {
                    _isFlashEnabled = value;
                    cameraView.ViewController?.UseFlash(value);
                    OnPropertyChanged("IsFlashEnabled");
                }
            }
        }

        protected BarcodeScannerResultHandler HandleScanResult;
        protected bool isEnabled = true;
        protected FrameLayout cameraLayout;
        protected BarcodeScannerView cameraView;
        protected FinderOverlayView finderOverlayView;
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly int REQUEST_PERMISSION_CODE = 200;

        public AndroidBarcodeCameraRenderer(Context context) : base(context)
        {
            SetupViews(context);
        }

        private void SetupViews(Context context)
        {
            cameraLayout = (FrameLayout)LayoutInflater
                      .FromContext(Context)
                      .Inflate(NativeBarcodeSDKRenderer.Droid.Resource.Layout.barcode_camera_view, null, false);

            // Here we retrieve the Camera View...
            cameraView = cameraLayout.FindViewById<BarcodeScannerView>(NativeBarcodeSDKRenderer.Droid.Resource.Id.barcode_camera);
        }

        private void StartDetection()
        {
            CheckPermissions();
            cameraView?.ViewController?.StartPreview();
            cameraView?.ViewController?.OnResume();
        }

        private void StopDetection()
        {
            isEnabled = false;
            cameraView?.ViewController?.OnPause();
            cameraView?.ViewController?.StopPreview();
        }

        /*
            This is probably the most important method that belongs to a ViewRenderer.
            You must override this in order to actually implement the renderer.
            OnElementChanged is called whenever the View or one of its properties have changed;
            this includes the initialization as well, therefore we initialize our native control here.
         */
        protected override void OnElementChanged(ElementChangedEventArgs<NativeBarcodeSDKRenderer.Views.BarcodeCameraView> e)
        {
            // The SetNativeControl method should be used to instantiate the native control,
            // and this method will also assign the control reference to the Control property
            SetNativeControl(cameraLayout);

            base.OnElementChanged(e);

            if (Control != null)
            {
                // The Element object is the instance of BarcodeCameraView as defined in the Forms
                // core project. We've defined some delegates there, and we'll bind to them here so that
                // these native calls will be executed whenever those methods will be called
                Element.StartDetectionHandler = (sender, e) =>
                {
                    StartDetection();
                };

                Element.StopDetectionHandler = (sender, e) =>
                {
                    StopDetection();
                };

                Element.SetBinding(IsFlashEnabledProperty, "IsFlashEnabled", BindingMode.TwoWay);
                Element.BindingContext = this;

                // Similarly, we have defined a delegate in our BarcodeCameraView implementation,
                // so that we can trigger it whenever the Scanner will return a valid result.
                HandleScanResult = Element.OnBarcodeScanResult;
                var detector = new ScanbotBarcodeScannerSDK(Context.GetActivity()).CreateBarcodeDetector();
                detector.ModifyConfig(new Function1Impl<BarcodeScannerConfigBuilder>((response) =>
                {
                    response.SetSaveCameraPreviewFrame(false);
                }));

                cameraView.InitCamera(new CameraUiSettings(false));
                BarcodeScannerViewWrapper.InitDetectionBehavior(cameraView,
                                                                detector,
                                                                new BarcodeDetectorResultHandler((result, error) => HandleFrameHandlerResult(result, error)),
                                                                this);
            }
        }

        private bool HandleSuccess(BarcodeScanningResult result)
        {
            if (result == null) { return false; }

            var outResult = new ScanbotBarcodeSDK.Forms.BarcodeResultBundle //ScanbotSDK.Xamarin.Forms.BarcodeScanningResult
            {
                Barcodes = result.BarcodeItems.ToFormsBarcodeList(),
                Image = result.PreviewFrame.ToImageSource()
            };

            HandleScanResult?.Invoke(outResult);
            return true;
        }

        private bool HandleFrameHandlerResult(BarcodeScanningResult result, SdkLicenseError error)
        {
            if (error != null)
            {
                cameraView.Post(() => Toast.MakeText(Context.GetActivity(), "License has expired!", ToastLength.Long).Show());
                return false;
            }

            if (result != null)
            {
                HandleSuccess(result);
            }

            return false;
        }

        private void CheckPermissions()
        {
            if (Context == null || Context.GetActivity() == null)
            {
                return;
            }

            var activity = Context.GetActivity();

            if (ContextCompat.CheckSelfPermission(activity, Manifest.Permission.Camera) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(activity, new string[] { Manifest.Permission.Camera }, REQUEST_PERMISSION_CODE);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnSelectionOverlayBarcodeClicked(BarcodeItem barcodeItem)
        {
            var outResult = new BarcodeResultBundle
            {
                Barcodes = new List<Barcode>() { barcodeItem.ToFormsBarcode() },
                Image = barcodeItem.Image.ToImageSource()
            };

            HandleScanResult.Invoke(outResult);
        }

        public void OnCameraOpen()
        {
            cameraView?.ViewController?.UseFlash(IsFlashEnabled);
            cameraView?.PostDelayed(() =>
            {
                // Uncomment to disable shutter sound (supported since Android 4.2+):
                // Please note that some devices may not allow disabling the camera shutter sound. 
                // If the shutter sound state cannot be set to the desired value, this method will be ignored.
                //cameraView.SetShutterSound(false);

                // Enable ContinuousFocus mode:
                cameraView.ViewController?.ContinuousFocus();
            }, 500);
        }

        public void OnPictureTaken(byte[] image, CaptureInfo captureInfo)
        {
            // get the image here
        }
    }

    // Here we define a custom BarcodeDetectorResultHandler. Whenever a result is ready, the frame handler
    // will call the Handle method on this object. To make this more flexible, we allow to
    // specify a delegate through the constructor.
    class BarcodeDetectorResultHandler : BarcodeDetectorResultHandlerWrapper
    {
        public delegate bool BarcodeResultHandlerDelegate(BarcodeScanningResult result, SdkLicenseError error);
        public readonly BarcodeResultHandlerDelegate handleResultFunc;

        public BarcodeDetectorResultHandler(BarcodeResultHandlerDelegate handleResultFunc)
        {
            this.handleResultFunc = handleResultFunc;
        }

        public override bool HandleResult(BarcodeScanningResult result, SdkLicenseError error)
        {
            handleResultFunc(result, error);
            return false;
        }
    }
}
