using System;
using IO.Scanbot.Sdk;
using IO.Scanbot.Sdk.Barcode;
using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.Barcode_scanner;
using IO.Scanbot.Sdk.Camera;
using static IO.Scanbot.Sdk.Camera.FrameHandlerResult;

namespace BarcodeScannerExample.Droid
{
    class BarcodeEventArgs : EventArgs
    {
        public BarcodeScanningResult Result { get; private set; }

        public BarcodeEventArgs(Java.Lang.Object value)
        {
            Result = (BarcodeScanningResult)value;
        }
    }

    class BarcodeResultDelegate : BarcodeDetectorResultHandlerWrapper
    {
        public EventHandler<BarcodeEventArgs> Success;

        public override bool HandleResult(BarcodeScanningResult result, SdkLicenseError error)
        {
            if (!MainActivity.SDK.LicenseInfo.IsValid)
            {
                return false;
            }
            if (error != null || result == null)
            {
                return false;
            }

            Success?.Invoke(this, new BarcodeEventArgs(result));
            return true;
        }
    }

    public class PictureTakenEventArgs : EventArgs
    {
        public byte[] Image { get; private set; }
        public int Orientation { get; private set; }

        public PictureTakenEventArgs(byte[] image, int orientation)
        {
            Image = image;
            Orientation = orientation;
        }
    }

    class PictureResultDelegate : PictureCallback
    {
        public EventHandler<PictureTakenEventArgs> PictureTaken;

        public override void OnPictureTaken(byte[] image, CaptureInfo captureInfo)
        {
            if (!MainActivity.SDK.LicenseInfo.IsValid)
            {
                return;
            }
            PictureTaken?.Invoke(this, new PictureTakenEventArgs(image, captureInfo.ImageOrientation));
        }
    }
}

