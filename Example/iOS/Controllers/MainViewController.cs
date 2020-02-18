﻿using System;
using ScanbotBarcodeSDK.iOS;
using UIKit;

namespace BarcodeScannerExample.iOS
{
    public class MainViewController : UIViewController
    {
        public MainView ContentView { get; set; }

        BarcodeResultReceiver receiver;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new MainView();
            View = ContentView;

            Title = "BARCODE SCANNER";

            receiver = new BarcodeResultReceiver();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ContentView.ClassicButton.TouchUpInside += OnClassicButtonClick;
            ContentView.RTUUIButton.TouchUpInside += OnRTUUIButtonClick;
            ContentView.RTUUIImageButton.TouchUpInside += OnRTUUIImageButtonClick;
            ContentView.LibraryButton.TouchUpInside += OnLibraryButtonClick;
            ContentView.CodeTypesButton.TouchUpInside += OnCodeTypeButtonClick;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ContentView.ClassicButton.TouchUpInside -= OnClassicButtonClick;
            ContentView.RTUUIButton.TouchUpInside -= OnRTUUIButtonClick;
            ContentView.RTUUIImageButton.TouchUpInside -= OnRTUUIImageButtonClick;
            ContentView.LibraryButton.TouchUpInside -= OnLibraryButtonClick;
            ContentView.CodeTypesButton.TouchUpInside -= OnCodeTypeButtonClick;
        }

        private void OnScanResultReceived(object sender, ScannerEventArgs e)
        {
            if (e.IsEmpty)
            {
                Console.WriteLine("Result is empty, returning");
                return;
            }

            e.Controller.DismissViewController(false, null);

            receiver.ResultsReceived -= OnScanResultReceived;

            SBSDKBarcodeScannerResult[] codes = null;
            if (e.Codes != null)
            {
                codes = e.Codes.ToArray();
            }
            var controller = new ScanResultListController(e.BarcodeImage, codes);
            NavigationController.PushViewController(controller, true);
        }

        private void OnClassicButtonClick(object sender, EventArgs e)
        {
            var controller = new ClassicScannerController();
            NavigationController.PushViewController(controller, true);
        }

        private void OnRTUUIButtonClick(object sender, EventArgs e)
        {
            OpenRTUUIBarcodeScanner(false);
        }

        private void OnRTUUIImageButtonClick(object sender, EventArgs e)
        {
            OpenRTUUIBarcodeScanner(true);
        }

        private async void OnLibraryButtonClick(object sender, EventArgs e)
        {
            UIImage image = await Scanbot.ImagePicker.iOS.ImagePicker.Instance.Pick();

            var scanner = new SBSDKBarcodeScanner(BarcodeTypes.Instance.AcceptedTypes.ToArray());

            SBSDKBarcodeScannerResult[] result = scanner.DetectBarCodesOnImage(image);

            var controller = new ScanResultListController(image, result);
            NavigationController.PushViewController(controller, true);
        }

        private void OnCodeTypeButtonClick(object sender, EventArgs e)
        {
            var controller = new BarcodeListController();
            NavigationController.PushViewController(controller, true);
        }

        void OpenRTUUIBarcodeScanner(bool withImage)
        {
            var configuration = SBSDKUIMachineCodeScannerConfiguration.DefaultConfiguration;
            configuration.UiConfiguration.FinderHeight = 0.5f;
            configuration.UiConfiguration.FinderWidth = 1f;

            if (withImage)
            {
                configuration.BehaviorConfiguration.BarcodeImageGenerationType =
                    SBSDKBarcodeImageGenerationType.CapturedImage;
            }

            receiver.WaitForImage = withImage;
            receiver.ResultsReceived += OnScanResultReceived;

            SBSDKUIBarcodeScannerViewController.PresentOn(
                this, BarcodeTypes.Instance.AcceptedTypes.ToArray(), configuration, receiver
            );
        }
    }
}
