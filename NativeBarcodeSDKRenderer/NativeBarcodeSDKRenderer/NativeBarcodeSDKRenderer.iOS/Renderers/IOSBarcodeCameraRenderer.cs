using System;
using System.ComponentModel;
using System.Linq;
using CoreAudioKit;
using CoreGraphics;
using NativeBarcodeSDKRenderer.iOS.Renderers;
using NativeBarcodeSDKRenderer.Views;
using ScanbotBarcodeSDK.Forms;
using ScanbotBarcodeSDK.Forms.iOS;
using ScanbotBarcodeSDK.iOS;
using StoreKit;
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
                Element.StartDetectionHandler = (sender, args1) =>
                {
                    cameraView.Start();
                };

                Element.StopDetectionHandler = (sender, args2) =>
                {
                    cameraView.Stop();
                };


                Element.SetBinding(BarcodeCameraView.IsFlashEnabledProperty, "IsFlashEnabled", BindingMode.TwoWay);
                Element.BindingContext = cameraView;
                // Selection overlay configuration
                cameraView.SetOverlayConfiguration(Element.OverlayConfiguration);
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

        public override void DidDetectBarcodes(SBSDKBarcodeScannerViewController controller, SBSDKBarcodeScannerResult[] codes, UIImage image)
        {
            OnDetect?.Invoke(codes);
        }

        public override bool ShouldDetectBarcodes(SBSDKBarcodeScannerViewController controller)
        {
            if (ScanbotSDK.IsLicenseValid)
            {
                return true;
            }
            else
            {
                (UIApplication.SharedApplication.Delegate as AppDelegate)?.ShowAlert("License Expired!", "Ok");
                return false;
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

        public SBSDKBarcodeScannerViewController Controller { get; private set; }
        public BarcodeScannerDelegate ScannerDelegate { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public IOSBarcodeCameraView(CGRect frame) : base(frame) { }

        public void Initialize(UIViewController parentViewController)
        {
            Controller = new SBSDKBarcodeScannerViewController(parentViewController, this);
            Controller.BarcodeImageGenerationType = SBSDKBarcodeImageGenerationType.None;
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;

            
        }

        // Set the overlay configuration to native controller.
        public void SetOverlayConfiguration(SelectionOverlayConfiguration overlayConfiguration)
        {
            if (overlayConfiguration?.Enabled == true)
            {
                Controller.SelectionOverlayEnabled = true;

                Controller.SelectionPolygonColor = overlayConfiguration.PolygonColor.ToUIColor();
                Controller.SelectionTextColor = overlayConfiguration.TextColor.ToUIColor();
                Controller.SelectionTextContainerColor = overlayConfiguration.TextContainerColor.ToUIColor();
                if (overlayConfiguration.HighlightedPolygonColor != null)
                {
                    Controller.SelectionHighlightedPolygonColor = overlayConfiguration.HighlightedPolygonColor?.ToUIColor();
                }

                if (overlayConfiguration.HighlightedTextColor != null)
                {
                    Controller.SelectionHighlightedTextColor = overlayConfiguration.HighlightedTextColor?.ToUIColor();
                }

                if (overlayConfiguration.HighlightedTextContainerColor != null)
                {
                    Controller.SelectionHighlightedTextContainerColor = overlayConfiguration.HighlightedTextContainerColor?.ToUIColor();
                }
            }
        }

        public void Stop()
        {
            Controller.RecognitionEnabled = false;
        }

        public void Start()
        {
            Controller.RecognitionEnabled = true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
