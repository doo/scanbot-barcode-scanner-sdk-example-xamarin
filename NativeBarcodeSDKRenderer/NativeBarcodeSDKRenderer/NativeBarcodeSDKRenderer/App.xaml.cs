using System;
using ScanbotBarcodeSDK.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NativeBarcodeSDKRenderer
{
    public partial class App : Application
    {
        public App ()
        {
            InitializeComponent();

            MainPage = new MainPage();

            var options = new InitializationOptions
            {
                LicenseKey = Common.ScanbotSDKConfiguration.LICENSE_KEY,
                LoggingEnabled = true,
                UseCameraXRtuUi = false,
                ErrorHandler = (status, feature) =>
                {
                    Console.WriteLine($"License error: {status}, {feature}");
                }
            };
            SBSDK.Initialize(options);
            Console.WriteLine(SBSDK.LicenseInfo);
        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }
    }
}

