using System;
using ScanbotBarcodeSDK.Forms;
using Xamarin.Forms;

namespace ScanbotBarcodeSDKFormsExample
{
    public partial class App : Application
    {
        public const string Key = null;

        public App()
        {
            InitializeComponent();

            var content = new MainPage();
            MainPage = new NavigationPage(content)
            {
                BarBackgroundColor = Color.FromRgb(200, 25, 60),
                BarTextColor = Color.White
            };

            NavigationPage.SetTitleView(content, new Label
            {
                Text = "BARCODE SCANNER",
                TextColor = Color.White,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });

            SBSDK.Initialize(new ScanbotBarcodeSDK.Forms.InitializationOptions
            {
                LicenseKey = Key,
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
