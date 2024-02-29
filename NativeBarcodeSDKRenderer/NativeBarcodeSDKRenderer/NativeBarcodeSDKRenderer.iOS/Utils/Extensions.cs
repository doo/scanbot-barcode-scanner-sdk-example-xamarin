using ScanbotBarcodeSDK.Forms;
using ScanbotBarcodeSDK.iOS;

namespace NativeBarcodeSDKRenderer.iOS.Utils
{
    public static class Extensions
	{
        public static SBSDKBarcodeOverlayFormat ToNative(this BarcodeDialogFormat overlayTextFormat)
        {
            switch (overlayTextFormat)
            {
                case BarcodeDialogFormat.None:
                    return SBSDKBarcodeOverlayFormat.None;
                case BarcodeDialogFormat.Code:
                    return SBSDKBarcodeOverlayFormat.Code;
                default:
                    return SBSDKBarcodeOverlayFormat.CodeAndType;
            }
        }
    }
}

