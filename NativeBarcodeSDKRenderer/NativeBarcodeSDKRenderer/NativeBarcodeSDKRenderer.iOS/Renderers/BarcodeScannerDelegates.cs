using ScanbotBarcodeSDK.iOS;

namespace NativeBarcodeSDKRenderer.iOS.Renderers
{
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

    internal class BarcodeTrackingOverlayDelegate : SBSDKBarcodeTrackingOverlayControllerDelegate
    {
        public delegate void DidTapOnBarcodeAROverlay(SBSDKBarcodeScannerResult barcode);
        public DidTapOnBarcodeAROverlay DidTapBarcodeOverlay;

        public override void BarcodeTrackingOverlay(SBSDKBarcodeTrackingOverlayController controller, SBSDKBarcodeScannerResult barcode)
        {
            DidTapBarcodeOverlay?.Invoke(barcode);
        }
    }
}