using System;
using ScanbotBarcodeSDK.Forms;
using Xamarin.Forms;

namespace ScanbotBarcodeSDKFormsExample
{
    public partial class App : Application
    {
        public const string Key = "<your-scanbot-license-key>";

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

            // TODO: Register your license
            SBSDK.LicenseManager.Register(Key);
        }

        private void OnLicenseError(object sender, LicenseEventArgs e)
        {
            Console.WriteLine($"License error: {e.Status}, {e.Feature}");
        }

        protected override void OnStart()
        {
            SBSDK.LicenseManager.OnLicenseError += OnLicenseError;
        }

        protected override void OnSleep()
        {
            SBSDK.LicenseManager.OnLicenseError -= OnLicenseError;
        }

    }
}
