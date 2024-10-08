﻿using System;
using System.Collections.Generic;
using ScanbotSDK = ScanbotBarcodeSDK.iOS.ScanbotSDKGlobal;
using ScanbotBarcodeSDK.iOS;
using UIKit;

namespace BarcodeScannerExample.iOS
{
    /// <summary>
    /// BatchBarcode Interaction interface
    /// </summary>
    interface IBatchBarcodeDelegateInteraction
    {
        /// <summary>
        /// Gets the viewController instance
        /// </summary>
        UIViewController ViewController { get; }
    }

    public class MainViewController : UIViewController, IBatchBarcodeDelegateInteraction
    {
        public MainView ContentView { get; set; }

        /// <summary>
        /// Interface implementation - returns the current instance
        /// </summary>
        public UIViewController ViewController => this;

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
            ContentView.RTUUIAROverlayButton.TouchUpInside += OnRTUUIAROverlayButtonClick;
            ContentView.LibraryButton.TouchUpInside += OnLibraryButtonClick;
            ContentView.CodeTypesButton.TouchUpInside += OnCodeTypeButtonClick;
            ContentView.StorageClearButton.TouchUpInside += OnClearStorageButtonClick;
            ContentView.LicenseInfoButton.TouchUpInside += OnLicenseInfoButtonClick;
            ContentView.RTUUIBatchBarcodeButton.TouchUpInside += OnRTUBatchBarcodeClicked;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ContentView.ClassicButton.TouchUpInside -= OnClassicButtonClick;
            ContentView.RTUUIButton.TouchUpInside -= OnRTUUIButtonClick;
            ContentView.RTUUIImageButton.TouchUpInside -= OnRTUUIImageButtonClick;
            ContentView.RTUUIAROverlayButton.TouchUpInside -= OnRTUUIAROverlayButtonClick;
            ContentView.LibraryButton.TouchUpInside -= OnLibraryButtonClick;
            ContentView.CodeTypesButton.TouchUpInside -= OnCodeTypeButtonClick;
            ContentView.StorageClearButton.TouchUpInside -= OnClearStorageButtonClick;
            ContentView.LicenseInfoButton.TouchUpInside -= OnLicenseInfoButtonClick;
            ContentView.RTUUIBatchBarcodeButton.TouchUpInside -= OnRTUBatchBarcodeClicked;
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
            if (!Alert.CheckLicense(this))
            {
                return;
            }
            var controller = new ClassicScannerController();
            NavigationController.PushViewController(controller, true);
        }

        private void OnRTUUIButtonClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this))
            {
                return;
            }
            OpenRTUUIBarcodeScanner(false);
        }

        private void OnRTUUIImageButtonClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this))
            {
                return;
            }
            OpenRTUUIBarcodeScanner(true);
        }

        private void OnRTUUIAROverlayButtonClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this))
            {
                return;
            }

            var defaultConfiguration = SBSDKUIBarcodeScannerConfiguration.DefaultConfiguration;

            defaultConfiguration.TrackingOverlayConfiguration.OverlayEnabled = true;
            defaultConfiguration.TrackingOverlayConfiguration.AutomaticSelectionEnabled = false;
            defaultConfiguration.TrackingOverlayConfiguration.TextColor = UIColor.Yellow;
            defaultConfiguration.TrackingOverlayConfiguration.PolygonColor = UIColor.Yellow;
            defaultConfiguration.TrackingOverlayConfiguration.TextContainerColor = UIColor.Black;

            defaultConfiguration.BehaviorConfiguration.AcceptedBarcodeTypes = BarcodeTypes.Instance.AcceptedTypes.ToArray();

            // On result received handler
            receiver.ResultsReceived += OnScanResultReceived;

            SBSDKUIBarcodeScannerViewController.PresentOn((UIViewController)this, defaultConfiguration, receiver);
        }

        private async void OnLibraryButtonClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this))
            {
                return;
            }
            UIImage image = await Scanbot.ImagePicker.iOS.ImagePicker.Instance.Pick();

            if (image != null)
            {
                var scanner = new SBSDKBarcodeScanner(BarcodeTypes.Instance.AcceptedTypes.ToArray());
                SBSDKBarcodeScannerResult[] result = scanner.DetectBarCodesOnImage(image);

                var controller = new ScanResultListController(image, result);
                NavigationController.PushViewController(controller, true);
            }
        }

        private void OnCodeTypeButtonClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this))
            {
                return;
            }
            var controller = new BarcodeListController();
            NavigationController.PushViewController(controller, true);
        }


        private void OnClearStorageButtonClick(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this))
            {
                return;
            }
            SBSDKUIBarcodeImageStorage.DefaultStorage.RemoveAll();
            Alert.Show(this, "Success", "Image storage cleared");
        }

        private void OnLicenseInfoButtonClick(object sender, EventArgs e)
        {
            var status = ScanbotSDK.LicenseStatus;
            var date = ScanbotSDK.LicenseExpirationDate;

            var message = $"License status is {status}";

            if (date != null)
            {
                message += $" until {date.ToDateTime()}";
            }

            Alert.Show(this, "Status", message);
        }

        /// <summary>
        /// Open the Barcode scanner RTU UI.
        /// </summary>
        /// <param name="withImage"></param>
        void OpenRTUUIBarcodeScanner(bool withImage)
        {
            var uiConfiguration = new SBSDKUIBarcodeScannerUIConfiguration();
            var textConfiguration = new SBSDKUIBarcodeScannerTextConfiguration();
            var behaviourConfiguration = new SBSDKUIBarcodeScannerBehaviorConfiguration();
            var cameraConfiguration = new SBSDKUICameraConfiguration();
            var selectionOverlayConfiguration = new SBSDKUIBarcodeTrackingOverlayConfiguration();

            behaviourConfiguration.AcceptedBarcodeTypes = BarcodeTypes.Instance.AcceptedTypes.ToArray();
            behaviourConfiguration.AdditionalParameters = new SBSDKBarcodeAdditionalParameters { MinimumTextLength = 6 };
            var configuration = new SBSDKUIBarcodeScannerConfiguration(uiConfiguration, textConfiguration, behaviourConfiguration, cameraConfiguration, selectionOverlayConfiguration);
            if (withImage)
            {
                configuration.BehaviorConfiguration.BarcodeImageGenerationType =
                    SBSDKBarcodeImageGenerationType.CapturedImage;
            }

            configuration.UiConfiguration.FinderAspectRatio = new SBSDKAspectRatio(1, 1);

            receiver.WaitForImage = behaviourConfiguration.BarcodeImageGenerationType == SBSDKBarcodeImageGenerationType.CapturedImage;

            // On result received handler
            receiver.ResultsReceived += OnScanResultReceived;

            SBSDKUIBarcodeScannerViewController.PresentOn((UIViewController)this, configuration, receiver);
        }

        /// <summary>
        /// Batch Barcode Scanner clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnRTUBatchBarcodeClicked(object sender, EventArgs e)
        {
            if (!Alert.CheckLicense(this))
            {
                return;
            }
            var batchBarcode = new BatchBarcodeDelegate(this);
            var configuration = SBSDKUIBarcodesBatchScannerConfiguration.DefaultConfiguration;
            configuration.UiConfiguration.FinderAspectRatio = new SBSDKAspectRatio(1, 0.5);
            configuration.BehaviorConfiguration.AcceptedBarcodeTypes = BarcodeTypes.Instance.AcceptedTypes.ToArray();

            batchBarcode.OpenBatchBarcodeScannerView(configuration);
        }
    }

    /// <summary>
    /// Batch Barcode Delegate implementation.
    /// </summary>
    internal class BatchBarcodeDelegate : SBSDKUIBarcodesBatchScannerViewControllerDelegate
    {
        IBatchBarcodeDelegateInteraction _interaction;
        /// <summary>
        /// BatchBarcodeDelegate Constructor
        /// </summary>
        /// <param name="interaction"></param>
        internal BatchBarcodeDelegate(IBatchBarcodeDelegateInteraction interaction)
        {
            _interaction = interaction;
        }

        /// <summary>
        /// Opens the BatchBarcodeScannerView
        /// </summary>
        internal void OpenBatchBarcodeScannerView(SBSDKUIBarcodesBatchScannerConfiguration configuration)
        {
            if (_interaction?.ViewController != null)
            {
                SBSDKUIBarcodesBatchScannerViewController.PresentOn(
                    _interaction?.ViewController, configuration, this
                );
            }
        }

        /// <summary>
        /// On ViewResults button click on BatchBarcodeScannerView
        /// Navigates to the result page.
        /// </summary>
        /// <param name="viewController"></param>
        /// <param name="barcodeResults"></param>
        public override void DidDetect(SBSDKUIBarcodesBatchScannerViewController viewController, SBSDKUIBarcodeMappedResult[] barcodeResults)
        {
            var resultViewController = new BatchBarcodeResultViewController();
            resultViewController.NavigateData(new List<SBSDKUIBarcodeMappedResult>(barcodeResults));
            _interaction?.ViewController?.NavigationController?.PushViewController(resultViewController, true);
        }
    }
}
