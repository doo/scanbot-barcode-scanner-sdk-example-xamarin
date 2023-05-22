
using IO.Scanbot.Sdk.Camera;
using Android.Content;
using Xamarin.Forms.Platform.Android;
using IO.Scanbot.Sdk.Barcode;
using AndroidX.Core.Content;
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using IO.Scanbot.Sdk.Barcode.Entity;
using Android.Widget;
using Xamarin.Forms;
using Android.Views;
using System.Collections.Generic;
using IO.Scanbot.Sdk.UI.Camera;
using NativeBarcodeSDKRenderers.Droid.Renderers;
using NativeBarcodeSDKRenderer.Droid.Renderers;
using IO.Scanbot.Sdk.Barcode_scanner;
using ScanbotBarcodeSDK.Forms.Droid;
using System.ComponentModel;
using IO.Scanbot.Sdk.Barcode.UI;

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
    class AndroidBarcodeCameraRenderer : ViewRenderer<NativeBarcodeSDKRenderer.Views.BarcodeCameraView, FrameLayout>, IBarcodeScannerViewCallback
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
                    cameraView?.ViewController?.UseFlash(value);
                    OnPropertyChanged("IsFlashEnabled");
                }
            }
        }

        protected NativeBarcodeSDKRenderer.Views.BarcodeCameraView.BarcodeScannerResultHandler HandleScanResult;
        protected FrameLayout cameraLayout;
        protected BarcodeScannerView cameraView;
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly int REQUEST_PERMISSION_CODE = 200;

        public AndroidBarcodeCameraRenderer(Context context) : base(context)
        {
            SetupViews(context);
        }

        private void SetupViews(Context context)
        {
            // We instantiate our views from the layout XML
            cameraLayout = (FrameLayout)LayoutInflater
                .FromContext(context)
                .Inflate(NativeBarcodeSDKRenderer.Droid.Resource.Layout.barcode_camera_view, null, false);

            // Here we retrieve the Camera View...
            cameraView = cameraLayout.FindViewById<BarcodeScannerView>(NativeBarcodeSDKRenderer.Droid.Resource.Id.barcode_camera);
        }

        private void StartDetection()
        {
            cameraView?.ViewController.StartPreview();
            cameraView?.ViewController?.OnResume();
            CheckPermissions();
        }

        private void StopDetection()
        {
            cameraView?.ViewController.StopPreview();
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
                // these native calls will be executed whenever those methods will be called.
                Element.OnResumeHandler = (sender, e) =>
                {
                    cameraView?.ViewController?.OnResume();
                };

                Element.OnPauseHandler = (sender, e) =>
                {
                    cameraView?.ViewController?.OnPause();
                };

                Element.StartDetectionHandler = (sender, e) =>
                {
                    StartDetection();
                };

                Element.StopDetectionHandler = (sender, e) =>
                {
                    StopDetection();
                };

                Element.SetBinding(NativeBarcodeSDKRenderer.Views.BarcodeCameraView.IsFlashEnabledProperty, "IsFlashEnabled", BindingMode.TwoWay);
                Element.BindingContext = this;

                // Similarly, we have defined a delegate in our BarcodeCameraView implementation,
                // so that we can trigger it whenever the Scanner will return a valid result.
                HandleScanResult = Element.OnBarcodeScanResult;

                // Here we create the BarcodeDetectorFrameHandler which will take care of detecting
                // barcodes in your video frames
                var detector = new ScanbotBarcodeScannerSDK(Context.GetActivity()).CreateBarcodeDetector();

                detector.ModifyConfig(new Function1Impl<BarcodeScannerConfigBuilder>((response) =>
                {
                    response.SetSaveCameraPreviewFrame(false);
                }));


                cameraView.InitCamera(new CameraUiSettings(false));
                cameraView.InitDetectionBehavior(detector, new BarcodeDetectorResultHandler(HandleFrameHandlerResult), this);

                if (Element?.OverlayConfiguration?.Enabled == true)
                {
                    cameraView.SelectionOverlayController.SetEnabled(Element.OverlayConfiguration.Enabled);
                    cameraView.SelectionOverlayController.SetPolygonColor(Element.OverlayConfiguration.PolygonColor.ToArgb());
                    cameraView.SelectionOverlayController.SetTextColor(Element.OverlayConfiguration.TextColor.ToArgb());
                    cameraView.SelectionOverlayController.SetTextContainerColor(Element.OverlayConfiguration.TextContainerColor.ToArgb());

                    if (Element.OverlayConfiguration.HighlightedPolygonColor != null)
                    {
                        cameraView.SelectionOverlayController.SetPolygonHighlightedColor(Element.OverlayConfiguration.HighlightedPolygonColor.ToArgb() ?? 0);
                    }

                    if (Element.OverlayConfiguration.HighlightedTextColor != null)
                    {
                        cameraView.SelectionOverlayController.SetTextHighlightedColor(Element.OverlayConfiguration.HighlightedTextColor.ToArgb() ?? 0);
                    }

                    if (Element.OverlayConfiguration.HighlightedTextContainerColor != null)
                    {
                        cameraView.SelectionOverlayController.SetTextContainerHighlightedColor(Element.OverlayConfiguration.HighlightedTextContainerColor.ToArgb() ?? 0);
                    }
                }

                // Finder view controller disable.
                cameraView?.FinderViewController?.SetFinderEnabled(false);
            }
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
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region BarcodeDetectorResultHandler

        private bool HandleFrameHandlerResult(FrameHandlerResult result)
        {
            if (result is FrameHandlerResult.Success success)
            {
                if (success.Value is BarcodeScanningResult barcodeResult)
                {
                    HandleSuccess(barcodeResult);
                }
            }
            else
            {
                cameraView.Post(() => Toast.MakeText(Context.GetActivity(), "License has expired!", ToastLength.Long).Show());
            }

            return false;
        }

        private bool HandleSuccess(BarcodeScanningResult result)
        {
            if (result == null) { return false; }

            var overlayEnabled = Element.OverlayConfiguration?.Enabled ?? false;
            if (overlayEnabled == false)
            {
                var outResult = new ScanbotBarcodeSDK.Forms.BarcodeResultBundle //ScanbotSDK.Xamarin.Forms.BarcodeScanningResult
                {
                    Barcodes = result.BarcodeItems.ToFormsBarcodeList(),
                    Image = result.PreviewFrame.ToImageSource()
                };

                HandleScanResult?.Invoke(outResult);
            }
            return true;
        }

        #endregion

        #region IBarcodeScannerViewCallback

        public void OnSelectionOverlayBarcodeClicked(BarcodeItem barcodeItem)
        {
            var outResult = new ScanbotBarcodeSDK.Forms.BarcodeResultBundle //ScanbotSDK.Xamarin.Forms.BarcodeScanningResult
            {
                Barcodes = new List<ScanbotBarcodeSDK.Forms.Barcode>() { barcodeItem.ToFormsBarcode() },
                Image = barcodeItem.Image.ToImageSource()
            };

            HandleScanResult?.Invoke(outResult);
        }

        public void OnCameraOpen()
        {
            cameraView.ViewController.UseFlash(IsFlashEnabled);
        }

        public void OnPictureTaken(byte[] image, CaptureInfo captureInfo)
        {

        }

        #endregion
    }

    // Here we define a custom BarcodeDetectorResultHandler. Whenever a result is ready, the frame handler
    // will call the Handle method on this object. To make this more flexible, we allow to
    // specify a delegate through the constructor.
    class BarcodeDetectorResultHandler : BarcodeDetectorFrameHandler.BarcodeDetectorResultHandler
    {
        public delegate bool HandleResultFunction(FrameHandlerResult result);
        public readonly HandleResultFunction handleResultFunc;

        public BarcodeDetectorResultHandler(HandleResultFunction handleResultFunc)
        {
            this.handleResultFunc = handleResultFunc;
        }

        public override bool Handle(FrameHandlerResult result)
        {
            handleResultFunc(result);
            return false;
        }
    }
}
