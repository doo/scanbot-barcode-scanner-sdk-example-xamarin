﻿using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using NativeBarcodeSDKRenderer.Common;
using ScanbotBarcodeSDK.iOS;
using UIKit;

namespace NativeBarcodeSDKRenderer.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // Most important line, else the Forms objects won't work.
            ScanbotBarcodeSDK.Forms.iOS.DependencyManager.Register();
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());
            Xamarin.Essentials.Platform.Init(() => Window.RootViewController);
            return base.FinishedLaunching(app, options);
        }

        /// <summary>
        /// Show message on top of the Root window
        /// </summary>
        /// <param name="message"></param>
        /// <param name="buttonTitle"></param>
        internal void ShowAlert(string message, string buttonTitle)
        {
            var alert = UIAlertController.Create("Alert", message, UIAlertControllerStyle.Alert);
            var action = UIAlertAction.Create(buttonTitle ?? "Ok", UIAlertActionStyle.Cancel, (obj) => { });
            alert.AddAction(action);
            Window?.RootViewController?.PresentViewController(alert, true, null);
        }
    }
}

