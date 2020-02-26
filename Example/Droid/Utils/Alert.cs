using System;
using Android.Content;
using IO.Scanbot.Sdk.Barcode_scanner;

namespace BarcodeScannerExample.Droid
{
    public class Alert
    {
        public static bool CheckLicense(Context context, ScanbotBarcodeScannerSDK sdk)
        {
            if (!sdk.LicenseInfo.IsValid)
            {
                Toast(context, "License invalid or expired");
            }

            return sdk.LicenseInfo.IsValid;
        }

        public static void Toast(Context context, string message)
        {
            Android.Widget.Toast.MakeText(context, message, Android.Widget.ToastLength.Long).Show();
        }
    }
}
