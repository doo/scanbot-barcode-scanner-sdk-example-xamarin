using Android;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android.Views;
using Android.Widget;

using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.UI.Camera;
using IO.Scanbot.Sdk.Barcode.UI;
using AndroidBarcode = IO.Scanbot.Sdk.Barcode.Entity.BarcodeItem;

using NativeBarcodeSDKRenderers.Droid.Renderers;
using NativeBarcodeSDKRenderer.Droid.Renderers;
using NativeBarcodeSDKRenderer.Views;

using ScanbotBarcodeSDK.Forms;
using ScanbotBarcodeSDK.Forms.Droid;
using System.ComponentModel;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Resource = NativeBarcodeSDKRenderer.Droid.Resource;

/*
    This is the Android Custom Renderer that will provide the actual implementation for BarcodeCameraView.
    We use the 'ExportRenderer' assembly directive to specify that we want to attach AndroidBarcodeCameraRenderer to
    BarcodeCameraView.

    Syntax:

    [assembly: ExportRenderer(typeof([FORMS_VIEW_CLASS]), typeof([CUSTOM_RENDERER_CLASS]))]

    ---
 */
[assembly: ExportRenderer(typeof(BarcodeCameraView), typeof(AndroidBarcodeCameraRenderer))]
namespace NativeBarcodeSDKRenderers.Droid.Renderers
{
    /*
       By extending 'ViewRenderer' we specify that we want our custom renderer to target 'BarcodeCameraView' and
       override it with our native view, which is a 'FrameLayout' in this case (see layout/barcode_camera_view.xml)
    */
    class AndroidBarcodeCameraRenderer : ViewRenderer<BarcodeCameraView, FrameLayout>, BarcodePolygonsView.IBarcodeHighlightDelegate, BarcodePolygonsView.IBarcodeAppearanceDelegate, INotifyPropertyChanged
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

        bool flashEnabled;
        protected FrameLayout cameraLayout;
        protected BarcodeScannerView cameraView;
        private readonly int REQUEST_PERMISSION_CODE = 200;
        BarcodeResultDelegate resultHandler;
        private bool showToast;
        public event PropertyChangedEventHandler PropertyChanged;

        public AndroidBarcodeCameraRenderer(Context context) : base(context)
        {
            SetupViews(context);
        }

        private void SetupViews(Context context)
        {

            // We instantiate our views from the layout XML
            cameraLayout = (FrameLayout)LayoutInflater
                .FromContext(context)
                .Inflate(Resource.Layout.barcode_camera_view, null, false);

            // Here we retrieve the Camera View...
            cameraView = cameraLayout.FindViewById<BarcodeScannerView>(Resource.Id.barcode_camera);
        }

        /*
            This is probably the most important method that belongs to a ViewRenderer.
            You must override this in order to actually implement the renderer.
            OnElementChanged is called whenever the View or one of its properties have changed;
            this includes the initialization as well, therefore we initialize our native control here.
         */
        protected override void OnElementChanged(ElementChangedEventArgs<BarcodeCameraView> e)
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
                    cameraView.ViewController.OnResume();
                };

                Element.OnPauseHandler = (sender, e) =>
                {
                    cameraView.ViewController.OnPause();
                };

                Element.StartDetectionHandler = (sender, e) =>
                {
                    cameraView.ViewController.OnResume();
                    CheckPermissions();
                };

                Element.StopDetectionHandler = (sender, e) =>
                {
                    cameraView.ViewController.OnPause();
                };

                Element.SetBinding(BarcodeCameraView.IsFlashEnabledProperty, "IsFlashEnabled", BindingMode.TwoWay);
                Element.BindingContext = this;

                // Here we create the BarcodeDetectorFrameHandler which will take care of detecting
                // barcodes in your video frames
                var detector = new IO.Scanbot.Sdk.Barcode_scanner.ScanbotBarcodeScannerSDK(Context.GetActivity()).CreateBarcodeDetector();
                detector.ModifyConfig(new Function1Impl<BarcodeScannerConfigBuilder>((response) =>
                {
                    response.SetSaveCameraPreviewFrame(false);
                }));

                cameraView.InitCamera(new CameraUiSettings(false));
                // result delegate
                resultHandler = new BarcodeResultDelegate();
                resultHandler.Success += OnBarcodeResult;

                // scanner delegates
                var scannerViewCallback = new BarcodeScannerViewCallback();
                scannerViewCallback.CameraOpen = OnCameraOpened;
                scannerViewCallback.SelectionOverlayBarcodeClicked += OnSelectionOverlayBarcodeClicked;

                BarcodeScannerViewWrapper.InitDetectionBehavior(cameraView, detector, resultHandler, scannerViewCallback);
                SetSelectionOverlayConfiguration();
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Registered Handlers

        public void OnCameraOpened()
        {
            cameraView.PostDelayed(delegate
            {
                cameraView.ViewController.UseFlash(flashEnabled);
                cameraView.ViewController.ContinuousFocus();
            }, 300);
        }

        private void OnSelectionOverlayBarcodeClicked(object sender, AndroidBarcode e)
        {
            var outResult = new BarcodeResultBundle
            {
                Barcodes = new List<Barcode> { e.ToFormsBarcode() },
                Image = e.Image.ToImageSource()
            };

            Element.OnBarcodeScanResult?.Invoke(outResult);
        }

        private void OnBarcodeResult(object sender, BarcodeEventArgs e)
        {
            if (!SBSDK.LicenseInfo.IsValid && !showToast)
            {
                showToast = true;
                cameraView.Post(() => Toast.MakeText(Context.GetActivity(), "License has expired!", ToastLength.Long).Show());
                return;
            }

            var overlayEnabled = Element.OverlayConfiguration?.Enabled ?? false;
            if (overlayEnabled == false || Element.OverlayConfiguration?.AutomaticSelectionEnabled == true)
            {
                var outResult = new BarcodeResultBundle
                {
                    Barcodes = e.Result.BarcodeItems.ToFormsBarcodeList(),
                    Image = e.Result.PreviewFrame.ToImageSource()
                };

                Element.OnBarcodeScanResult?.Invoke(outResult);
            }
        }

        #endregion

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


        #region Overlay Configuration

        private void SetSelectionOverlayConfiguration()
        {
            if (Element?.OverlayConfiguration?.Enabled == true)
            {
                cameraView.SelectionOverlayController.SetEnabled(Element.OverlayConfiguration.Enabled);
                cameraView.SelectionOverlayController.SetBarcodeHighlightedDelegate(this);
                cameraView.SelectionOverlayController.SetBarcodeAppearanceDelegate(this);
            }
        }

        public bool ShouldHighlight(AndroidBarcode barcodeItem)
        {
            return Element?.OverlayConfiguration?.AutomaticSelectionEnabled ?? false;
        }

        public BarcodePolygonsView.BarcodePolygonStyle GetPolygonStyle(BarcodePolygonsView.BarcodePolygonStyle defaultStyle, AndroidBarcode barcodeItem)
        {
            return GetOverlayPolygonStyle(defaultStyle);
        }

        public BarcodePolygonsView.BarcodeTextViewStyle GetTextViewStyle(BarcodePolygonsView.BarcodeTextViewStyle defaultStyle, AndroidBarcode barcodeItem)
        {
            return GetOverlayTextStyle(defaultStyle);
        }

        private BarcodePolygonsView.BarcodePolygonStyle GetOverlayPolygonStyle(BarcodePolygonsView.BarcodePolygonStyle defaultStyle)
        {
            if (Element.OverlayConfiguration != null)
            {
                var polygonStyle = new BarcodePolygonsView.BarcodePolygonStyle(drawPolygon: defaultStyle.DrawPolygon,
                   useFill: false, // default fill is true. Please set true if you want to fill color into the barcode polygon.
                   useFillHighlighted: defaultStyle.UseFillHighlighted,
                   cornerRadius: defaultStyle.CornerRadius,
                   strokeWidth: defaultStyle.StrokeWidth,
                   strokeColor: Element.OverlayConfiguration.PolygonColor?.ToAndroid() ?? defaultStyle.StrokeColor,
                   strokeHighlightedColor: Element.OverlayConfiguration.HighlightedPolygonColor?.ToAndroid() ?? defaultStyle.StrokeHighlightedColor,
                   fillColor: defaultStyle.FillColor,
                   fillHighlightedColor: defaultStyle.FillHighlightedColor,
                   shouldDrawShadows: defaultStyle.ShouldDrawShadows);

                return polygonStyle;
            }
            return defaultStyle;
        }

        private BarcodePolygonsView.BarcodeTextViewStyle GetOverlayTextStyle(BarcodePolygonsView.BarcodeTextViewStyle defaultStyle)
        {
            if (Element.OverlayConfiguration != null)
            {
                  var textStyle = new BarcodePolygonsView.BarcodeTextViewStyle(
                    textColor: Element.OverlayConfiguration.TextColor?.ToAndroid() ?? defaultStyle.TextColor,
                    textHighlightedColor: Element.OverlayConfiguration.HighlightedTextColor?.ToAndroid() ?? defaultStyle.TextHighlightedColor,
                    textContainerColor: Element.OverlayConfiguration.TextContainerColor?.ToAndroid() ?? defaultStyle.TextContainerColor,
                    textContainerHighlightedColor: Element.OverlayConfiguration.HighlightedTextContainerColor?.ToAndroid() ?? defaultStyle.TextContainerHighlightedColor,
                    textFormat: Element.OverlayConfiguration.OverlayTextFormat?.ToNative() ?? defaultStyle.TextFormat);
                return textStyle;
            }
            return defaultStyle;
        }

        #endregion
    }

    public static class Extension
    {
        // --------------------------------
        // Overlay Text Format
        // --------------------------------
        public static BarcodeOverlayTextFormat ToAndroid(this BarcodeDialogFormat format)
        {
            switch (format)
            {
                case BarcodeDialogFormat.None:
                    return BarcodeOverlayTextFormat.None;
                case BarcodeDialogFormat.Code:
                    return BarcodeOverlayTextFormat.Code;
                default:
                    return BarcodeOverlayTextFormat.CodeAndType;
            }
        }
    }
}
