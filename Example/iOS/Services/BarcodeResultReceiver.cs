using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using ScanbotBarcodeSDK.iOS;
using UIKit;

namespace BarcodeScannerExample.iOS
{
    public class ScannerEventArgs : EventArgs
    {
        public List<SBSDKBarcodeScannerResult> Codes { get; private set; }

        public UIImage BarcodeImage { get; private set; }

        public NSUrl ImageUrl { get; private set; }

        public bool HasImage => BarcodeImage != null;

        public bool IsEmpty { get => !HasImage && Codes.Count == 0; }

        public UIViewController Controller { get; set; }

        public ScannerEventArgs(List<SBSDKBarcodeScannerResult> codes, UIImage image, NSUrl imageUrl)
        {
            Codes = codes;
            BarcodeImage = image;
            ImageUrl = ImageUrl;
        }

        internal void Update(UIImage barcodeImage)
        {
            BarcodeImage = barcodeImage;
        }
    }


    public class ClassicScannerReceiver : SBSDKBarcodeScannerViewControllerDelegate
    {
        public EventHandler<ScannerEventArgs> ResultReceived;

        public override void BarcodeScannerController(
            SBSDKBarcodeScannerViewController controller, SBSDKBarcodeScannerResult[] codes)
        {
            ResultReceived?.Invoke(this, new ScannerEventArgs(codes.ToList(), null, null));
        }

        public override void BarcodeScannerController(
            SBSDKBarcodeScannerViewController controller, UIImage barcodeImage)
        {
            ResultReceived?.Invoke(this, new ScannerEventArgs(null, barcodeImage, null));
        }

        public override bool BarcodeScannerControllerShouldDetectBarcodes(SBSDKBarcodeScannerViewController controller)
        {
            return true;
        }
    }

    public class BarcodeResultReceiver : SBSDKUIBarcodeScannerViewControllerDelegate
    {
        public bool WaitForImage { get; set; }

        public EventHandler<ScannerEventArgs> ResultsReceived;

        public override void QrBarcodeDetectionViewController(
            SBSDKUIBarcodeScannerViewController viewController, SBSDKBarcodeScannerResult[] barcodeResults)
        {
            Invoke(viewController, barcodeResults, null, null);
        }

        ScannerEventArgs args;

        public override void QrBarcodeDetectionViewController(
            SBSDKUIBarcodeScannerViewController viewController, UIImage barcodeImage, NSUrl imageURL)
        {
            args.Controller = viewController;
            args.Update(barcodeImage);

            if (WaitForImage)
            {
                ResultsReceived?.Invoke(this, args);
            }
        }

        void Invoke(SBSDKUIBarcodeScannerViewController viewController,
            SBSDKBarcodeScannerResult[] barcodeResults, UIImage barcodeImage, NSUrl imageURL)
        {
            List<SBSDKBarcodeScannerResult> result = null;
            if (barcodeResults != null)
            {
                result = barcodeResults.ToList();
            }
            args = new ScannerEventArgs(result, barcodeImage, imageURL);
            args.Controller = viewController;

            if (!WaitForImage)
            {
                ResultsReceived?.Invoke(this, args);
            }

        }
    }
}
