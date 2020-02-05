using Foundation;
using ScanbotBarcodeSDK.iOS;
using UIKit;

namespace BarcodeScannerExample.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            ScanbotSDK.SetLoggingEnabled(true);
            ScanbotSDK.SetLicense(SDKLicense.Key);
            
            return true;
        }

    }
}

