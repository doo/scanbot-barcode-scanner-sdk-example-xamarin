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

            scannerController.BarcodeAccumulatedFramesCount = 15;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            receiver.ResultReceived += OnScanResultReceived;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            receiver.ResultReceived -= OnScanResultReceived;
        }

        private void OnScanResultReceived(object sender, ScannerEventArgs e)
        {
            Console.WriteLine("Results received");
        }
    }
}

