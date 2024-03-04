using System;
using System.Threading.Tasks;
using NativeBarcodeSDKRenderer.Common;
using ScanbotBarcodeSDK.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace NativeBarcodeSDKRenderer
{
    public partial class MainPage : ContentPage
    {
        private bool isDetectionOn;
        /// <summary>
        /// Is Barcode scanning is On or Off.
        /// </summary>
        private bool IsDetectionOn
        {
            get => isDetectionOn;
            set
            {
                isDetectionOn = value;
                scanButton.Text = value ? "STOP SCANNING" : "START SCANNING";
                RefreshCamera();
            }
        }

        /// <summary>
        /// Get the License of the SDK.
        /// </summary>
        private bool IsLicenseValid => SBSDK.LicenseInfo.IsValid;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            SetupViews();
        }

        /// <summary>
        /// Set Up UI on initial launch
        /// </summary>
        private void SetupViews()
        {
            cameraView.OnBarcodeScanResult = HandleBarcodeScanningResult;
            cameraView.OverlayConfiguration = new SelectionOverlayConfiguration(
                automaticSelectionEnabled: true, overlayFormat: BarcodeDialogFormat.Code,
                polygon: Color.Black, text: Color.Black, textContainer: Color.White,
                highlightedPolygonColor: Color.Red, highlightedTextColor: Color.Red, highlightedTextContainerColor: Color.Black);
            cameraView.ImageGenerationType = BarcodeImageGenerationType.FromVideoFrame;

            SetupIOSAppearance();
        }

        /// <summary>
        /// OnAppearing - Invoked on every Page forground
        /// </summary>
        async protected override void OnAppearing()
        {
            base.OnAppearing();

            scanButton.Clicked += OnScanButtonPressed;
            infoButton.Clicked += OnInfoButtonPressed;

            if (!await CameraPermissionAllowed())
            {
                IsDetectionOn = false;
            }

            // Update UI with IsDetection flag status
            RefreshCamera();

            if (string.IsNullOrWhiteSpace(ScanbotSDKConfiguration.LICENSE_KEY))
            {
                await ShowTrialLicenseAlert();
            }
        }

        /// <summary>
        /// OnDisappearing - Invoked on every Page background
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            scanButton.Clicked -= OnScanButtonPressed;
            infoButton.Clicked -= OnInfoButtonPressed;

            PauseCamera();
        }

        /// Scanning Button click event.
        /// Checks for Camera Permission, License and invokes scanning feature.
        private async void OnScanButtonPressed(object sender, EventArgs e)
        {
            if (!await CameraPermissionAllowed())
                return;

            if (!IsLicenseValid && !IsDetectionOn)
            {
                ShowExpiredLicenseAlert();
                return;
            }

            IsDetectionOn = !IsDetectionOn;
            cameraView.IsVisible = true;
            cameraViewImage.IsVisible = false;
        }

        private void OnInfoButtonPressed(object sender, EventArgs e)
        {
            DisplayAlert("Info", IsLicenseValid ? "Your SDK License is valid." : "Your SDK License has expired.", "Close");
        }

        private void ShowExpiredLicenseAlert()
        {
            DisplayAlert("Error", "Your SDK license has expired", "Close");
        }

        async private Task ShowTrialLicenseAlert()
        {
            await DisplayAlert("Welcome", "You are using the Trial SDK License. The SDK will be active for one minute.", "Close");
        }

        private void SetupIOSAppearance()
        {
            if (Device.RuntimePlatform != Device.iOS) { return; }

            var safeInsets = On<iOS>().SafeAreaInsets();
            safeInsets.Bottom = 0;
            Padding = safeInsets;
        }

        private void PauseCamera()
        {
            IsDetectionOn = false;
            cameraView.Pause();
        }

        // Updates UI when the Barcode scanning is turned on/off.
        private void RefreshCamera()
        {
            if (IsDetectionOn)
            {
                cameraView.StartDetection();
                scanButton.Text = "STOP SCANNING";
                resultsPreviewLayout.BackgroundColor = Color.White;
                resultsLabel.Text = string.Empty;
            }
            else
            {
                scanButton.Text = "START SCANNING";
                resultsPreviewLayout.BackgroundColor = Color.FromHex("#d2d2d2");
                cameraView.StopDetection();
                ToggleFlash(false);
            }
        }

        /// Check for Camera permissions.
        private async Task<bool> CameraPermissionAllowed()
        {
            var isAllowed = false;
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            switch (status)
            {
                case PermissionStatus.Unknown:  // Defult permission status For iOS is Unknown
                    {
                        await Permissions.RequestAsync<Permissions.Camera>();
                        isAllowed = await Permissions.CheckStatusAsync<Permissions.Camera>() == PermissionStatus.Granted;
                    }
                    break;

                case PermissionStatus.Denied: // Default permission status for Android is Denied
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        status = await Permissions.RequestAsync<Permissions.Camera>();
                        isAllowed = await Permissions.CheckStatusAsync<Permissions.Camera>() == PermissionStatus.Granted;
                    }

                    if (!isAllowed)
                    {
                        await DisplayAlert("Alert", "Please turn on the camera permissions from setting screen, to turn on scanning feature.", "Ok");
                    }
                    break;

                case PermissionStatus.Disabled:
                    break;

                case PermissionStatus.Granted:
                    isAllowed = true;
                    break;

                case PermissionStatus.Restricted:
                    break;
            }
            return isAllowed;
        }

        // ImageButton element clicked event. Handles Flash Light on/off.
        void FlashButtonClicked(System.Object sender, System.EventArgs e)
        {
            if (!IsDetectionOn)
                return;

            ToggleFlash(!cameraView.IsFlashEnabled);
        }

        /// <summary>
        /// Toggles the Flash button UI relecting On/Off and the camera view IsFlashEnabled property.
        /// </summary>
        /// <param name="isOn">Bool flag, if set to 'true' turns on the flash and 'false' to turn off the flash.</param>
        private void ToggleFlash(bool isOn)
        {
            cameraView.IsFlashEnabled = isOn;
            if (isOn)
            {
                imgButtonFlash.BackgroundColor = Color.Yellow;
            }
            else
            {
                imgButtonFlash.BackgroundColor = Color.Transparent;
            }
        }

        private void HandleBarcodeScanningResult(BarcodeResultBundle result)
        {
            string text = string.Empty;
            Barcode barcodeItem = null;
            foreach (Barcode barcode in result.Barcodes)
            {
                text += string.Format("{0} ({1})\n", barcode.Text, barcode.Format.ToString().ToUpper());
                barcodeItem = barcode;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                resultsLabel.Text = text;
                if (cameraView.ImageGenerationType == BarcodeImageGenerationType.CapturedImage)
                {
                    IsDetectionOn = false;
                    cameraView.IsVisible = false;
                    cameraViewImage.IsVisible = true;
                    cameraViewImage.Source = barcodeItem?.Image;
                    cameraViewImage.Aspect = Aspect.AspectFit;
                }
            });
        }
    }
}


