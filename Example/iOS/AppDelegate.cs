using Foundation;
using ScanbotBarcodeSDK.iOS;
using UIKit;

namespace BarcodeScannerExample.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public static readonly UIColor ScanbotRed = UIColor.FromRGB(200, 25, 60);

        public UINavigationController Controller { get; set; }

        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            ScanbotSDK.SetLoggingEnabled(true);

            // TRIAL license key!
            const string licenseKey =
              "OR1EO+sD5mejzso88coeSsACkfmi6b" +
              "D2DNOnaMwSPdrKl2Lg1DYRNnaZbkD4" +
              "fEHS7q1+UrXa7saimbp5Rsqxq5jgf/" +
              "Pv4Yzt5aaULnfax9VbIJmmL+qVcmiV" +
              "xx9lzP8CyDE3mRh3ylWG/rdfnt2EQi" +
              "iPiTZoj8LEjKF54uv4Ll4J9xih+Yd1" +
              "Nzl3+9y/TEAnWJGyGY7QjJ058gGVS8" +
              "rIYlvNG+Zbc4rmSq3c3NerNfbX0gMW" +
              "h7szr2Dr85OV9sx1uDM9wAdmmgsLds" +
              "/LPIp+0Qmkszqi92df8tH8+Anx7bZS" +
              "XFF7y2w+ghc1V/scFZQI+ygc8FNtlr" +
              "OgMDfoDQSaDQ==\nU2NhbmJvdFNESw" +
              "ppby5zY2FuYm90LmV4YW1wbGUuc2Rr" +
              "LmJhcmNvZGUueGFtYXJpbgoxNjA4OD" +
              "U0Mzk5CjUxMgoz\n";

            ScanbotSDK.SetLicense(licenseKey);

            UIViewController initial = new MainViewController();
            Controller = new UINavigationController(initial);

            // Navigation bar background color
            Controller.NavigationBar.BarTintColor = ScanbotRed;
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

