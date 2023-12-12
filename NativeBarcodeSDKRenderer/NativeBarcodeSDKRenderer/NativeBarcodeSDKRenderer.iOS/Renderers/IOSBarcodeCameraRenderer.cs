using CoreGraphics;

using NativeBarcodeSDKRenderer.iOS.Renderers;
using NativeBarcodeSDKRenderer.Views;

using ScanbotBarcodeSDK.Forms;
using ScanbotBarcodeSDK.Forms.iOS;
using ScanbotBarcodeSDK.iOS;
using System.ComponentModel;
using System.Linq;

using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BarcodeCameraView), typeof(IOSBarcodeCameraRenderer))]
namespace NativeBarcodeSDKRenderer.iOS.Renderers
{
    class IOSBarcodeCameraRenderer : ViewRenderer<BarcodeCameraView, IOSBarcodeCameraView>
    {
        private bool isInitialized = false;

        private IOSBarcodeCameraView cameraView;
        private UIViewController CurrentViewController
        {
            get
            {
                if (UIApplication.SharedApplication.Windows.First() is UIWindow window)
                {
                    return window.RootViewController;
                }

                return null;
            }
        }

        private BarcodeCameraView.BarcodeScannerResultHandler barcodeScannerResultHandler;

        public IOSBarcodeCameraRenderer() : base() { }

        public bool Flash { get; set; }

        protected override void OnElementChanged(ElementChangedEventArgs<BarcodeCameraView> e)
        {
            if (CurrentViewController == null) { return; }

            double x = e.NewElement.X;
            double y = e.NewElement.Y;
            double width = e.NewElement.WidthRequest;
            double height = e.NewElement.HeightRequest;

            cameraView = new IOSBarcodeCameraView(new CGRect(x, y, width, height));
            SetNativeControl(cameraView);

            if (Control != null)
            {
                Element.OnResumeHandler = (sender, e1) =>
                {
                    cameraView.Start();
                };

                Element.StartDetectionHandler = (sender, e2) =>
                {
                    cameraView.Start();
                };

                Element.OnPauseHandler = (sender, e3) =>
                {
                    cameraView.Stop();
                };

                Element.StopDetectionHandler = (sender, e4) =>
                {
                    cameraView.Stop();
                };

                Element.SetBinding(BarcodeCameraView.IsFlashEnabledProperty, "IsFlashEnabled", BindingMode.TwoWay);
                Element.BindingContext = cameraView;
            }

            base.OnElementChanged(e);
        }

        private void HandleBarcodeScannerResults(SBSDKBarcodeScannerResult[] codes)
        {
            barcodeScannerResultHandler?.Invoke(new BarcodeResultBundle()
            {
                Barcodes = codes.ToFormsBarcodes()
            });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            if (Control == null) { return; }
            if (!isInitialized)
            {
                FindAndInitialiseView();
            }
        }

        // Find the View from Navigation heirarchy and initialise it.
        private void FindAndInitialiseView()
        {
            var viewController = CurrentViewController?.ChildViewControllers?.First();

            // If application has a Navigation Controller
            if (viewController is UINavigationController navigationController)
            {
                InitialiseView(navigationController.VisibleViewController);
            }
            else if (viewController is UITabBarController tabBarController)
            {
                // It is itself a Page renderer.
                InitialiseView(tabBarController.SelectedViewController);
            }
            else
            {   // If application has no Navigation Controller OR TabBarController
                InitialiseView(viewController);
            }
        }

        /// Initialise the Camera View.
        private void InitialiseView(UIViewController visibleViewController)
        {
            PageRenderer pageRendererViewController = null;
            if (visibleViewController is PageRenderer) // In case of TabBedPage ViewController
            {
                pageRendererViewController = visibleViewController as PageRenderer;
            }
            else if (visibleViewController?.ChildViewControllers?.First() is PageRenderer) // Navigation/Single page
            {
                pageRendererViewController = visibleViewController.ChildViewControllers.First() as PageRenderer;
            }

            if (pageRendererViewController != null)
            {
                cameraView.Initialize(pageRendererViewController);
                cameraView.SetBarcodeConfigurations(Element);
                cameraView.ScannerDelegate.OnDetect = HandleBarcodeScannerResults;
                barcodeScannerResultHandler = Element.OnBarcodeScanResult;
                isInitialized = true;
            }
        }
    }

    // Since we cannot directly inherit from SBSDKBarcodeScannerViewControllerDelegate in our ViewRenderer,
    // we have created this wrapper class to allow binding to its events through the use of delegates
    class BarcodeScannerDelegate : SBSDKBarcodeScannerViewControllerDelegate
    {
        public delegate void OnDetectHandler(SBSDKBarcodeScannerResult[] codes);
        public OnDetectHandler OnDetect;

        public override void DidDetectBarcodes(SBSDKBarcodeScannerViewController controller, SBSDKBarcodeScannerResult[] codes)
        {
            OnDetect?.Invoke(codes);
        }
    }

    /// Native iOS Barcode CameraView using iOS controller.
    class IOSBarcodeCameraView : UIView, System.ComponentModel.INotifyPropertyChanged
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
                if (Controller != null)
                {
                    Controller.FlashLightEnabled = value;
                    _isFlashEnabled = value;
                    OnPropertyChanged("IsFlashEnabled");
                }
            }
        }

        public SBSDKBarcodeScannerViewController Controller { get; private set; }
        public BarcodeScannerDelegate ScannerDelegate { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public IOSBarcodeCameraView(CGRect frame) : base(frame) { }

        public void Initialize(UIViewController parentViewController)
        {
            Controller = new SBSDKBarcodeScannerViewController(parentViewController, this);
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;
        }

        internal void SetBarcodeConfigurations(BarcodeCameraView element)
        {
            Controller.BarcodeImageGenerationType = element.ImageGenerationType.ToNative();
            SetSelectionOverlayConfiguration(element.OverlayConfiguration);
        }

        private void SetSelectionOverlayConfiguration(SelectionOverlayConfiguration configuration)
        {
            if (configuration != null && configuration.Enabled)
            {
                var overlayConfiguration = new SBSDKBarcodeTrackingOverlayConfiguration();

                var polygonStyle = new SBSDKBarcodeTrackedViewPolygonStyle();
                polygonStyle.PolygonColor = configuration.PolygonColor.ToUIColor();
                polygonStyle.PolygonSelectedColor = configuration.HighlightedPolygonColor?.ToUIColor();

                // use below properties if you want to set background color to the polygon. As of now they are set to clear
                // eg: to show translucent color over barcode. 
                polygonStyle.PolygonBackgroundColor = UIColor.Clear;
                polygonStyle.PolygonBackgroundSelectedColor = UIColor.Clear;

                var textStyle = new SBSDKBarcodeTrackedViewTextStyle();
                textStyle.TextColor = configuration.TextColor.ToUIColor();
                textStyle.SelectedTextColor = configuration.HighlightedTextColor?.ToUIColor();
                textStyle.TextBackgroundColor = configuration.TextContainerColor.ToUIColor();
                textStyle.TextBackgroundSelectedColor = configuration.HighlightedTextContainerColor?.ToUIColor();

                overlayConfiguration.IsAutomaticSelectionEnabled = configuration.AutomaticSelectionEnabled;
                overlayConfiguration.TextStyle = textStyle;
                overlayConfiguration.PolygonStyle = polygonStyle;

                Controller.IsTrackingOverlayEnabled = configuration.Enabled;
                Controller.TrackingOverlayController.Configuration = overlayConfiguration;
            }
        }

        public void Stop()
        {
            Controller.RecognitionEnabled = false;
            Controller.FreezeCamera();
        }

        public void Start()
        {
            Controller.RecognitionEnabled = true;
            Controller.UnfreezeCamera();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class Extension
    {
        public static SBSDKBarcodeImageGenerationType ToNative(this BarcodeImageGenerationType imageGenerationType)
        {
            switch (imageGenerationType)
            {
                case BarcodeImageGenerationType.None:
                    return SBSDKBarcodeImageGenerationType.None;
                case BarcodeImageGenerationType.CapturedImage:
                    return SBSDKBarcodeImageGenerationType.CapturedImage;
                case BarcodeImageGenerationType.FromVideoFrame:
                    return SBSDKBarcodeImageGenerationType.FromVideoFrame;
                default:
                    return SBSDKBarcodeImageGenerationType.None;
            }
        }
    }
}
