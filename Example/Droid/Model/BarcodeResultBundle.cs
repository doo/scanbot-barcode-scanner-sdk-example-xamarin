using System;
using IO.Scanbot.Sdk.Barcode.Entity;

namespace BarcodeScannerExample.Droid
{
    public class BarcodeResultBundle
    {
        public static BarcodeResultBundle Instance { get; set; }

        public static BarcodeItem SelectedBarcodeItem { get; set; }


        public BarcodeScanningResult ScanningResult { get; set; }

        public string ImagePath { get; set; }

        public string PreviewPath { get; set; }

        public BarcodeResultBundle() { }

        public BarcodeResultBundle(BarcodeScanningResult result)
        {
            ScanningResult = result;
        }

    }
}
