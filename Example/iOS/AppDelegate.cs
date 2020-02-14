using Foundation;
using ScanbotBarcodeSDK.iOS;
using UIKit;

namespace BarcodeScannerExample.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public UINavigationController Controller { get; set; }

        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            ScanbotSDK.SetLoggingEnabled(true);
            // TODO: Initialize SDK with correct license
            //ScanbotSDK.SetLicense(SDKLicense.Key);

            UIViewController initial = new MainViewController();
            Controller = new UINavigationController(initial);

            // Navigation bar background color
            Controller.NavigationBar.BarTintColor = UIColor.FromRGB(200, 25, 60);
            // Back button color
            Controller.NavigationBar.TintColor = UIColor.White;
            // Title color
            Controller.NavigationBar.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.White,
                Font = UIFont.FromName("HelveticaNeue", 16),
            };
            Controller.NavigationBar.Translucent = false;
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            Window.RootViewController = Controller;

            Window.MakeKeyAndVisible();

            return true;
        }

    }
}

