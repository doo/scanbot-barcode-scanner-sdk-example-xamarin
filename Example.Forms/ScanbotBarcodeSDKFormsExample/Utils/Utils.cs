using System.IO;
using System.Threading.Tasks;
using ScanbotBarcodeSDK.Forms;
using Xamarin.Forms;

namespace ScanbotBarcodeSDKFormsExample
{
    public class Utils
    {
        public static bool CheckLicense(ContentPage context)
        {
            if (!SBSDK.LicenseInfo.IsValid)
            {
                Alert(context, "Invalid license!", "License expired or invalid, mate");
            }

            return SBSDK.LicenseInfo.IsValid;
        }

        static async void Alert(ContentPage context, string title, string message)
        {
            await context.DisplayAlert(title, message, "Close");
        }

        public static ImageSource Copy(ImageSource original)
        {
            var streamImageSource = (StreamImageSource)original;
            var cancellationToken = System.Threading.CancellationToken.None;
            Task<Stream> task = streamImageSource.Stream(cancellationToken);
            Stream stream = task.Result;
            return ImageSource.FromStream(() => stream);

        }
    }
}
