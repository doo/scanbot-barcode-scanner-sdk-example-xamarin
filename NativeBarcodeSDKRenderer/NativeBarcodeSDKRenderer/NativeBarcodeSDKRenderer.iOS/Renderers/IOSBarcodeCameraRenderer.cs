using CoreGraphics;
using NativeBarcodeSDKRenderer.iOS.Delegates;
using NativeBarcodeSDKRenderer.iOS.Renderers;
using NativeBarcodeSDKRenderer.iOS.Utils;
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

            base.OnElementChanged(e);

            Element.OnResumeHandler = (sender, e1) =>
            {
                cameraView.Controller.UnfreezeCamera();
                cameraView.ScannerDelegate.isScanning = true;
            };

            Element.StartDetectionHandler = (sender, e2) =>
            {
                cameraView.Controller.UnfreezeCamera();
                cameraView.ScannerDelegate.isScanning = true;
            };

            Element.OnPauseHandler = (sender, e3) =>
            {
                cameraView.Controller.FreezeCamera();
                cameraView.ScannerDelegate.isScanning = false;
            };

            Element.StopDetectionHandler = (sender, e4) =>
            {
                cameraView.Controller.FreezeCamera();
                cameraView.ScannerDelegate.isScanning = false;
            };


            Element.SetBinding(BarcodeCameraView.IsFlashEnabledProperty, "IsFlashEnabled", BindingMode.TwoWay);
            Element.BindingContext = cameraView;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            if (Control == null) { return; }
            if (!cameraView.IsInitialised)
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
            }
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

        public bool initialised = false;
        public SBSDKBarcodeScannerViewController Controller { get; private set; }
        public BarcodeScannerDelegate ScannerDelegate { get; private set; }
        internal bool IsInitialised = false;
        private BarcodeCameraView element;
        public event PropertyChangedEventHandler PropertyChanged;

        public IOSBarcodeCameraView(CGRect frame) : base(frame) { }

        public void Initialize(UIViewController parentViewController)
        {
            initialised = true;
            Controller = new SBSDKBarcodeScannerViewController(parentViewController, this);
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;
        }

        internal void SetBarcodeConfigurations(BarcodeCameraView element)
        {
            this.element = element;
            Controller.AcceptedBarcodeTypes = SBSDKBarcodeType.AllTypes;
            Controller.BarcodeImageGenerationType = element.ImageGenerationType.ToNative();
            ScannerDelegate.OnDetect = HandleBarcodeScannerResults;
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
                textStyle.TrackingOverlayTextFormat = configuration.OverlayTextFormat.ToNative();
                textStyle.TextColor = configuration.TextColor.Value.ToUIColor();
                textStyle.SelectedTextColor = configuration.HighlightedTextColor.Value.ToUIColor();
                textStyle.TextBackgroundColor = configuration.TextContainerColor.Value.ToUIColor();
                textStyle.TextBackgroundSelectedColor = configuration.HighlightedTextContainerColor?.ToUIColor();

                overlayConfiguration.IsAutomaticSelectionEnabled = configuration.AutomaticSelectionEnabled;
                overlayConfiguration.IsSelectable = true;
                overlayConfiguration.TextStyle = textStyle;
                overlayConfiguration.PolygonStyle = polygonStyle;

                Controller.IsTrackingOverlayEnabled = configuration.Enabled;
                Controller.TrackingOverlayController.Configuration = overlayConfiguration;
                Controller.TrackingOverlayController.Delegate = new BarcodeTrackingOverlayDelegate
                {
                    DidTapBarcodeOverlay = HandleDidTapOnBarcodeOverlay
                };
            }
        }

        private void HandleBarcodeScannerResults(SBSDKBarcodeScannerResult[] codes)
        {
            var returnResults = true;
            if (Controller.IsTrackingOverlayEnabled)
            {
                // return results if tracking overlay is set to automatic selection true
                returnResults = Controller.TrackingOverlayController?.Configuration?.IsAutomaticSelectionEnabled ?? false;
            }

            if (returnResults)
            {
                this.element?.OnBarcodeScanResult.Invoke(new BarcodeResultBundle()
                {
                    Barcodes = codes.ToFormsBarcodes()
                });
            }
        }

        private void HandleDidTapOnBarcodeOverlay(SBSDKBarcodeScannerResult barcode)
        {
            this.element?.OnBarcodeScanResult.Invoke(new BarcodeResultBundle()
            {
                Barcodes = new System.Collections.Generic.List<Barcode> { barcode.ToFormsBarcode() }
            });
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
