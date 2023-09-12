﻿using System;
using System.Collections.Generic;
using System.Linq;
using ScanbotBarcodeSDK.Forms;

namespace ScanbotBarcodeSDKFormsExample
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

        public List<BarcodeFormat> All =>
            Enum.GetValues(typeof(BarcodeFormat)).Cast<BarcodeFormat>().ToList();

        private BarcodeTypes()
        {
            var original = All;
           
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
