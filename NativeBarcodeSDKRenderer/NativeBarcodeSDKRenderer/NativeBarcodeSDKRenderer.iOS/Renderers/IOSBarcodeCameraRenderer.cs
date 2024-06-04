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
        protected override void OnElementChanged(ElementChangedEventArgs<BarcodeCameraView> e)
        {
            if (UIApplication.SharedApplication.KeyWindow?.RootViewController == null) { return; }

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


            Element.SetBinding(BarcodeCameraView.IsFlashEnabledProperty, nameof(IOSBarcodeCameraView.IsFlashEnabled), BindingMode.TwoWay);
            Element.BindingContext = cameraView;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            if (Control == null) { return; }
            if (!cameraView.IsInitialised)
            {
                cameraView.Initialize(Element);
                cameraView.SetBarcodeConfigurations();
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
        
        internal bool IsInitialised;
        public SBSDKBarcodeScannerViewController Controller { get; private set; } 
        public BarcodeScannerDelegate ScannerDelegate { get; private set; }
        private BarcodeCameraView element;
        public event PropertyChangedEventHandler PropertyChanged;

        public IOSBarcodeCameraView(CGRect frame) : base(frame) { }


        public void Initialize(BarcodeCameraView element)
        {
            this.element = element;
            IsInitialised = true;
            Controller = new SBSDKBarcodeScannerViewController();
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;

            AddBarcodeScannerToParentViewController();
        }
        
        private void AddBarcodeScannerToParentViewController()
        {
            Controller.View.Frame = Bounds;

#if IOS16_0_OR_GREATER
            // On iOS 16+ and macOS 13+ the SBSDKBarcodeScannerViewController has to be added to a parent ViewController, otherwise the transport controls won't be displayed.
            var viewController = GetParentPageViewControllerInElementsTree(element?.ParentView) ?? UIApplication.SharedApplication.KeyWindow?.RootViewController;
        
            // If we don't find the viewController, assume it's not Shell and still continue, the transport controls will still be displayed
            if (viewController?.View is null)
            {
                // Zero out the safe area insets of the SBSDKBarcodeScannerViewController
                UIEdgeInsets insets = viewController.View.SafeAreaInsets;
                Controller.AdditionalSafeAreaInsets =
                    new UIEdgeInsets(insets.Top * -1, insets.Left, insets.Bottom * -1, insets.Right);


                // Add the View from the SBSDKBarcodeScannerViewController to the parent ViewController
                viewController.AddChildViewController(Controller);
                viewController.View.AddSubview(Controller.View);
            }
#endif
            AddSubview(Controller.View);
        }
        
        private UIViewController GetParentPageViewControllerInElementsTree( Element virtualViewParentElement)
        {
            // Traverse up the element tree to find the nearest parent page
            Element currentElement = virtualViewParentElement;
            while (currentElement != null && !(currentElement is Page))
            {
                currentElement = currentElement.Parent;
            }

            // If a Page is found, get its corresponding ViewController
            return (currentElement as Page)?.GetRenderer()?.ViewController;
        }

        internal void SetBarcodeConfigurations()
        {
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

                Controller.TrackingOverlayController.Configuration.IsAutomaticSelectionEnabled =
                       configuration.AutomaticSelectionEnabled ?? false;

                if (configuration.PolygonColor != null)
                {
                    Controller.TrackingOverlayController.Configuration.PolygonStyle.PolygonColor =
                    configuration.PolygonColor.Value.ToUIColor();
                }

                if (configuration.HighlightedPolygonColor != null)
                {
                    Controller.TrackingOverlayController.Configuration.PolygonStyle.PolygonSelectedColor =
                        configuration.HighlightedPolygonColor.Value.ToUIColor();
                }

                if (configuration.PolygonBackgroundColor != null)
                {
                    Controller.TrackingOverlayController.Configuration.PolygonStyle.PolygonBackgroundColor =
                    configuration.PolygonBackgroundColor.Value.ToUIColor();
                }

                if (configuration.PolygonBackgroundSelectedColor != null)
                {
                    Controller.TrackingOverlayController.Configuration.PolygonStyle.PolygonBackgroundSelectedColor =
                    configuration.PolygonBackgroundSelectedColor.Value.ToUIColor();
                }

                if (configuration.TextColor != null)
                {
                    Controller.TrackingOverlayController.Configuration.TextStyle.TextColor =
                    configuration.TextColor.Value.ToUIColor();
                }

                if (configuration.HighlightedTextColor != null)
                {
                    Controller.TrackingOverlayController.Configuration.TextStyle.SelectedTextColor =
                    configuration.HighlightedTextColor.Value.ToUIColor();
                }

                if (configuration.TextContainerColor != null)
                {
                    Controller.TrackingOverlayController.Configuration.TextStyle.TextBackgroundColor =
                    configuration.TextContainerColor.Value.ToUIColor();
                }

                if (configuration.HighlightedTextContainerColor != null)
                {
                    Controller.TrackingOverlayController.Configuration.TextStyle.TextBackgroundSelectedColor =
                    configuration.HighlightedTextContainerColor.Value.ToUIColor();
                }

                if (configuration.OverlayTextFormat != null)
                {
                    Controller.TrackingOverlayController.Configuration.TextStyle.TrackingOverlayTextFormat =
                    configuration.OverlayTextFormat.Value.ToNative();
                }

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
