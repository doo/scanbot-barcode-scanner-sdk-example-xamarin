﻿using NativeBarcodeSDKRenderer.iOS.Utils;
using ScanbotBarcodeSDK.Forms;
using ScanbotBarcodeSDK.iOS;

namespace NativeBarcodeSDKRenderer.iOS.Delegates
{

    // Since we cannot directly inherit from SBSDKBarcodeScannerViewControllerDelegate in our ViewRenderer,
    // we have created this wrapper class to allow binding to its events through the use of delegates
    class BarcodeScannerDelegate : SBSDKBarcodeScannerViewControllerDelegate
    {
        internal bool isScanning = true;
        public delegate void OnDetectHandler(SBSDKBarcodeScannerResult[] codes);
        public OnDetectHandler OnDetect;

        public override void DidDetectBarcodes(SBSDKBarcodeScannerViewController controller, SBSDKBarcodeScannerResult[] codes)
        {
            if (controller.BarcodeImageGenerationType == SBSDKBarcodeImageGenerationType.CapturedImage)
            {
                isScanning = false; // it will restrict further scans and stop scanning when the image is captured.
            }
            OnDetect?.Invoke(codes);
        }

        public override bool ShouldDetectBarcodes(SBSDKBarcodeScannerViewController controller)
        {
            if (!SBSDK.LicenseInfo.IsValid)
            {
                ViewUtils.ShowAlert("License Expired!", "Ok");
                return false;
            }

            return isScanning;
        }
    }

    internal class BarcodeTrackingOverlayDelegate : SBSDKBarcodeTrackingOverlayControllerDelegate
    {
        public delegate void DidTapOnBarcodeAROverlay(SBSDKBarcodeScannerResult barcode);
        public DidTapOnBarcodeAROverlay DidTapBarcodeOverlay;

        public override void DidTapOnBarcode(SBSDKBarcodeTrackingOverlayController controller, SBSDKBarcodeScannerResult barcode)
        {
            DidTapBarcodeOverlay?.Invoke(barcode);
        }
    }
}
