using System;
using System.Collections.Generic;
using System.Linq;
using ScanbotBarcodeSDK.iOS;
using UIKit;

namespace BarcodeScannerExample.iOS
{
    public class ScannerEventArgs : EventArgs
    {
        public List<SBSDKBarcodeScannerResult> Codes { get; private set; }

        public UIImage BarcodeImage { get; private set; }

        public bool HasImage => BarcodeImage != null;

        public bool IsEmpty { get => !HasImage && Codes.Count == 0; }

        public UIViewController Controller { get; set; }

        public ScannerEventArgs(List<SBSDKBarcodeScannerResult> codes, UIImage image)
        {
            Codes = codes;
            BarcodeImage = image;
        }

        internal void Update(UIImage barcodeImage)
        {
            BarcodeImage = barcodeImage;
        }
    }


    public class ClassicScannerReceiver : SBSDKBarcodeScannerViewControllerDelegate
    {
        public EventHandler<ScannerEventArgs> ResultReceived;

        public override bool ShouldDetectBarcodes(SBSDKBarcodeScannerViewController controller)
        {
            return true;
        }

        public override void DidDetectBarcodes(SBSDKBarcodeScannerViewController controller, SBSDKBarcodeScannerResult[] codes)
        {
            ResultReceived?.Invoke(this, new ScannerEventArgs(codes.ToList(), codes.First()?.BarcodeImage));
        }
    }

    public class BarcodeResultReceiver : SBSDKUIBarcodeScannerViewControllerDelegate
    {
        public bool WaitForImage { get; set; }

        public EventHandler<ScannerEventArgs> ResultsReceived;

        public override void DidDetect(
            SBSDKUIBarcodeScannerViewController viewController, SBSDKBarcodeScannerResult[] barcodeResults)
        {
            Invoke(viewController, barcodeResults, barcodeResults.First()?.BarcodeImage);
        }

        ScannerEventArgs args;

        void Invoke(SBSDKUIBarcodeScannerViewController viewController, SBSDKBarcodeScannerResult[] barcodeResults, UIImage barcodeImage)
        {
            List<SBSDKBarcodeScannerResult> result = null;

            if (barcodeResults != null)
            {
                result = barcodeResults.ToList();
            }

            args = new ScannerEventArgs(result, barcodeImage)
            {
                Controller = viewController
            };


            ResultsReceived?.Invoke(this, args);
        }
    }
}
