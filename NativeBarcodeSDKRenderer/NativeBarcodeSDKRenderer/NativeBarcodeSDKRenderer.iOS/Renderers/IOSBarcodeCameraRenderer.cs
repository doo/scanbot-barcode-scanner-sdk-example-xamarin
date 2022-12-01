using System.Linq;
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

            if (CurrentViewController.ChildViewControllers.First() is PageRenderer pageRendererVc) {
                cameraView.Initialize(pageRendererVc);
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

        public IOSBarcodeCameraView(CGRect frame) : base(frame) {}

        public void Initialize(UIViewController parentViewController) {
            Controller = new SBSDKBarcodeScannerViewController(parentViewController, this);
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;
        }
    }
}
