using System;
using Android.App;
using Android.Runtime;
using IO.Scanbot.Sdk.Barcode_scanner;

namespace BarcodeScannerExample.Droid
{
    [Application(LargeHeap = true)]
    public class MainApplication : Application
    {
        // TRIAL license key!
        const string licenseKey =
          "OR1EO+sD5mejzso88coeSsACkfmi6b" +
          "D2DNOnaMwSPdrKl2Lg1DYRNnaZbkD4" +
          "fEHS7q1+UrXa7saimbp5Rsqxq5jgf/" +
          "Pv4Yzt5aaULnfax9VbIJmmL+qVcmiV" +
          "xx9lzP8CyDE3mRh3ylWG/rdfnt2EQi" +
          "iPiTZoj8LEjKF54uv4Ll4J9xih+Yd1" +
          "Nzl3+9y/TEAnWJGyGY7QjJ058gGVS8" +
          "rIYlvNG+Zbc4rmSq3c3NerNfbX0gMW" +
          "h7szr2Dr85OV9sx1uDM9wAdmmgsLds" +
          "/LPIp+0Qmkszqi92df8tH8+Anx7bZS" +
          "XFF7y2w+ghc1V/scFZQI+ygc8FNtlr" +
          "OgMDfoDQSaDQ==\nU2NhbmJvdFNESw" +
          "ppby5zY2FuYm90LmV4YW1wbGUuc2Rr" +
          "LmJhcmNvZGUueGFtYXJpbgoxNjA4OD" +
          "U0Mzk5CjUxMgoz\n";

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
