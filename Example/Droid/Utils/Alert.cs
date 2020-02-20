using System;
using Android.Content;

namespace BarcodeScannerExample.Droid
{
    public class Alert
    {
        public static void Toast(Context context, string message)
        {
            Android.Widget.Toast.MakeText(context, message, Android.Widget.ToastLength.Long).Show();
        }
    }
}
