using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using ScanbotBarcodeSDK.Forms.iOS;
using UIKit;

namespace ScanbotBarcodeSDKFormsExample.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            DependencyManager.Register();
            Scanbot.ImagePicker.Forms.iOS.DependencyManager.Register();

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
