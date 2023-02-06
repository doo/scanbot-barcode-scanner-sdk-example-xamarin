using System;
using System.Linq;
using CoreAudioKit;
using CoreGraphics;
using NativeBarcodeSDKRenderer.iOS.Renderers;
using NativeBarcodeSDKRenderer.Views;
using ScanbotBarcodeSDK.Forms;
using ScanbotBarcodeSDK.Forms.iOS;
using ScanbotBarcodeSDK.iOS;
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

        /// <summary>
        /// Find the View from Navigation heirarchy and initialise it.
        /// </summary>
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

        /// <summary>
        /// Initialise the Camera View.
        /// </summary>
        /// <param name="pageRendererViewController"></param>
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

    class IOSBarcodeCameraView : UIView
    {
        public SBSDKBarcodeScannerViewController Controller { get; private set; }
        public BarcodeScannerDelegate ScannerDelegate { get; private set; }

        public IOSBarcodeCameraView(CGRect frame) : base(frame) { }

        public void Initialize(UIViewController parentViewController)
        {
            Controller = new SBSDKBarcodeScannerViewController(parentViewController, this);
            Controller.BarcodeImageGenerationType = SBSDKBarcodeImageGenerationType.None;
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;
        }

        public void Stop()
        {
            Controller.RecognitionEnabled = false;
        }

        public void Start()
        {
            Controller.RecognitionEnabled = true;
        }
    }
}
