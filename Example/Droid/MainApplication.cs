using System;
using Android.App;
using Android.Runtime;
using IO.Scanbot.Sdk.Barcode_scanner;

namespace BarcodeScannerExample.Droid
{
    [Application(LargeHeap = true)]
    public class MainApplication : Application
    {
        // TRIAL license key
        const string licenseKey =
          "UCYCVgHFWmLmnF+vo/P3Lz+2Id8+E3" +
          "6g6HfEPeA2yIOFVthrE3SxA4cmOjLN" +
          "9s1gAZ+a9kSNTtWvISmkYVv/lDbqI4" +
          "ZDiBDb9olABB7tzE4LujyCgzr0LfmQ" +
          "Nl/CXBmFKe/PI6mgg2NA5yTjBvbvEY" +
          "73/HyVElbBJ8Lgs0vL3urLhP8hf9/h" +
          "DXJYyBS6/jyCgInrUbrbI1DIlPCZBj" +
          "7Rhdm+Zz6Ds1I4mItiqjsCLxyxVCi2" +
          "Ue30rz3YD1DwhZkuZLq7CtGVH46tf/" +
          "YP/oRSKLg6MUlgFixBaxt14lj+4l5m" +
          "emVPsWII3Yd03lunbbagL0167A+2gF" +
          "tlg3H+zWOxQQ==\nU2NhbmJvdFNESw" +
          "ppby5zY2FuYm90LmV4YW1wbGUuc2Rr" +
          "LmJhcmNvZGUueGFtYXJpbgoxNjE0Mj" +
          "k3NTk5CjUxMgoy\n";

        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        { }

        public override void OnCreate()
        {
            base.OnCreate();

            var initializer = new ScanbotBarcodeScannerSDKInitializer();
            initializer.WithLogging(true);
            initializer.License(this, licenseKey);
            initializer.Initialize(this);
        }
    }
}
