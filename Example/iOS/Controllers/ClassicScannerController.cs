using System;
using ScanbotBarcodeSDK.iOS;
using UIKit;

namespace BarcodeScannerExample.iOS
{
    public class ClassicScannerController : UIViewController
    {
        SBSDKBarcodeScannerViewController scannerController;

        ClassicScannerReceiver receiver;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "CLASSICAL COMPONENT";

            scannerController = new SBSDKBarcodeScannerViewController(this, View);

            receiver = new ClassicScannerReceiver();
            scannerController.Delegate = receiver;
            receiver.ResultReceived += OnScanResultReceived;

            scannerController.BarcodeAccumulatedFramesCount = 15;
        }

        private void OnScanResultReceived(object sender, ScannerEventArgs e)
        {
            Console.WriteLine("Results received");
            receiver.ResultReceived -= OnScanResultReceived;

            SBSDKBarcodeScannerResult[] codes = null;
            if (e.Codes != null)
            {
                codes = e.Codes.ToArray();
            }
            var controller = new ScanResultListController(e.BarcodeImage, codes);
            NavigationController.PushViewController(controller, true);
        }
    }
}

