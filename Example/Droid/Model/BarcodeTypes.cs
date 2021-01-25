using System;
using System.Collections.Generic;
using System.Linq;
using IO.Scanbot.Sdk.Barcode.Entity;

namespace BarcodeScannerExample.Droid
{
    public class BarcodeTypes
    {
        public static BarcodeTypes Instance { get; private set; } = new BarcodeTypes();

        public Dictionary<BarcodeFormat, bool> List { get; private set; } = new Dictionary<BarcodeFormat, bool>();

        public List<BarcodeFormat> AcceptedTypes
        {
            get
            {
                var result = new List<BarcodeFormat>();
                foreach (var item in List)
                {
                    if (item.Value)
                    {
                        result.Add(item.Key);
                    }
                }

                return result;
            }
        }

        public bool IsChecked(BarcodeFormat lastCheckedFormat)
        {
            return AcceptedTypes.Contains(lastCheckedFormat);
        }

        private BarcodeTypes()
        {
            var original = BarcodeFormat.Values().ToList();
            // MsiPlessey is beta and not recommened for production yet!
            original.Remove(BarcodeFormat.MsiPlessey);
            foreach (var item in original)
            {
                List.Add(item, true);
            }
        }

        public void Update(BarcodeFormat type, bool value)
        {
            List[type] = value;
        }

    }
}
