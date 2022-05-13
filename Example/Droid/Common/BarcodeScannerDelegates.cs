using System;
using IO.Scanbot.Sdk.Barcode;
using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.Barcode_scanner;
using IO.Scanbot.Sdk.Camera;

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

    class BarcodeResultDelegate : BarcodeDetectorFrameHandler.BarcodeDetectorResultHandler
    {
        public EventHandler<BarcodeEventArgs> Success;

        public override bool Handle(FrameHandlerResult p0)
        {
            if (!MainActivity.SDK.LicenseInfo.IsValid) {
                return false;
            }
            var success = (FrameHandlerResult.Success)p0;
            if (success != null && success.Value != null)
            {
                var value = (BarcodeScanningResult)success.Value;
                Success?.Invoke(this, new BarcodeEventArgs(success.Value));
                return true;
            }

            return false;
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

