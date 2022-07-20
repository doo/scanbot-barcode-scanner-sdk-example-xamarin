using System.Linq;
using CoreGraphics;
using Foundation;
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

            cameraView = new IOSBarcodeCameraView(CurrentViewController, new CGRect(x, y, width, height));
            SetNativeControl(cameraView);

            base.OnElementChanged(e);

            if (Control == null) { return; }

            cameraView.ScannerDelegate.OnDetect = HandleBarcodeScannerResults;

            barcodeScannerResultHandler = Element.OnBarcodeScanResult;
        }

        private void HandleBarcodeScannerResults(SBSDKBarcodeScannerResult[] codes)
        {
            barcodeScannerResultHandler?.Invoke(new BarcodeResultBundle()
            {
                Barcodes = codes.ToFormsBarcodes()
            });
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

        public override bool ShouldDetectBarcodes(SBSDKBarcodeScannerViewController controller)
        {
            return true;
        }
    }

    class IOSBarcodeCameraView : UIView
    {
        public SBSDKBarcodeScannerViewController Controller { get; private set; }
        public BarcodeScannerDelegate ScannerDelegate { get; private set; }

        public IOSBarcodeCameraView(UIViewController parent, CGRect frame) : base(frame)
        {
            Controller = new SBSDKBarcodeScannerViewController(parent, this);
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;
        }
    }
}
