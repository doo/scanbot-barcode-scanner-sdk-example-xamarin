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

            scannerController = new SBSDKBarcodeScannerViewController(this, View);

            receiver = new ClassicScannerReceiver();
            scannerController.Delegate = receiver;
            receiver.ResultReceived += OnScanResultReceived;

            scannerController.BarcodeAccumulatedFramesCount = 15;
            
            Flash = new FlashButton();
            View.AddSubview(Flash);
            nfloat size = 55;
            nfloat padding = 10;
            Flash.Frame = new CGRect(padding, padding, size, size);
            Flash.Click += (sender, e) =>
            {
                var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
                if (devices.Length == 0)
                {
                    Console.WriteLine("No video capture devices available");
                    return;
                }
                var device = devices[0];
                if (device.HasTorch && device.HasFlash)
                {
                    NSError error = null;
                    device.LockForConfiguration(out error);
                    if (e.Enabled)
                    {
                        device.SetTorchModeLevel((float)AVCaptureTorchMode.On, out error);
                    }
                    else
                    {
                        device.TorchMode = AVCaptureTorchMode.Off;
                    }
                    device.UnlockForConfiguration();
                }
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

