using UIKit;

namespace NativeBarcodeSDKRenderer.iOS.Utils
{
    public class ViewUtils
	{
        /// <summary>
        /// Show message on top of the Root window
        /// </summary>
        /// <param name="message"></param>
        /// <param name="buttonTitle"></param>
        internal static void ShowAlert(string message, string buttonTitle)
        {
            var alert = UIAlertController.Create("Alert", message, UIAlertControllerStyle.Alert);
            var action = UIAlertAction.Create(buttonTitle ?? "Ok", UIAlertActionStyle.Cancel, (obj) => { });
            alert.AddAction(action);
            var window = (UIApplication.SharedApplication.Delegate as AppDelegate).Window;
            window?.RootViewController?.PresentViewController(alert, true, null);
        }
    }
}

