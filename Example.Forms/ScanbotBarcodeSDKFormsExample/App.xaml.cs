using System;
using ScanbotBarcodeSDK.Forms;
using Xamarin.Forms;

namespace ScanbotBarcodeSDKFormsExample
{
    public partial class App : Application
    {
        /*
         * TODO Add the license key here.
         * Please note: The Scanbot SDK will run without a license key for one minute per session!
         * After the trial period has expired all Scanbot SDK functions as well as the UI components will stop working
         * or may be terminated. You have to restart the app to get another trial period.
         * You can get an unrestricted, "no-strings-attached" 30-day trial license key for free.
         * Please submit the trial license form (https://scanbot.io/en/sdk/demo/trial) on our website by using
         * the app identifier "io.scanbot.example.sdk.barcode.xamarin.forms" of this example app or of your app.
         */
        private const string LICENSE_KEY = null;

        public App()
        {
            InitializeComponent();

            var content = new MainPage();
            MainPage = new NavigationPage(content)
            {
                BarBackgroundColor = Color.FromRgb(200, 25, 60),
                BarTextColor = Color.White
            };

            SBSDK.Initialize(new ScanbotBarcodeSDK.Forms.InitializationOptions
            {
                LicenseKey = LICENSE_KEY,
                LoggingEnabled = true,
                ErrorHandler = (status, feature) =>
                {
                    var message = $"Error! Status: {status}; Your license is missing the feature: {feature}";
                    Console.WriteLine(message);
                }
            });
        }

    }
}
