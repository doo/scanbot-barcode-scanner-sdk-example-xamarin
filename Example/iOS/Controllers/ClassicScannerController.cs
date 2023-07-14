using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using ScanbotBarcodeSDK.iOS;
using UIKit;

namespace BarcodeScannerExample.iOS
{
    public class ClassicScannerController : UIViewController
    {
        SBSDKBarcodeScannerViewController scannerController;

        ClassicScannerReceiver receiver;

        public FlashButton Flash { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "CLASSICAL COMPONENT";
            scannerController = new SBSDKBarcodeScannerViewController(this, View)
            {
                AcceptedBarcodeTypes = BarcodeTypes.Instance.AcceptedTypes.ToArray()
            };

            receiver = new ClassicScannerReceiver();
            scannerController.Delegate = receiver;
            receiver.ResultReceived += OnScanResultReceived;
            
            Flash = new FlashButton();
            View.AddSubview(Flash);
            View.BackgroundColor = UIColor.Black;

            nfloat size = 55;
            nfloat padding = 10;
            Flash.Frame = new CGRect(padding, padding, size, size);
            Flash.Click += (sender, e) =>
            {
                scannerController.FlashLightEnabled = e.Enabled;
            };
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

            var navigation = NavigationController;

            navigation.PopViewController(false);
            navigation.PushViewController(controller, true);
        }
    }
}

