using System;
using Android.App;
using Android.Runtime;
using Android.Util;
using IO.Scanbot.Sdk.Barcode_scanner;
using NativeBarcodeSDKRenderer.Common;
using ScanbotBarcodeSDK.Forms;
using ScanbotBarcodeSDK.Forms.Droid;

namespace NativeBarcodeSDKRenderer.Droid
{
#if DEBUG
    [Application(Debuggable = true, LargeHeap = true, Theme = "@style/MainTheme")]
#else
    [Application(Debuggable = false, LargeHeap = true, Theme = "@style/AppTheme")]
#endif
    public class MainApplication : Application
    {

        static readonly string LOG_TAG = typeof(MainApplication).Name;

        // Use a custom temp storage directory for demo purposes.
        public static ScanbotOperations operations;

        // Initializers
        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
        public MainApplication() { }

        // Application Lifecycle
        public override void OnCreate()
        {
            base.OnCreate();

            operations = new ScanbotOperations();

            Log.Debug(LOG_TAG, "Initializing Scanbot SDK...");
            var initializer = new ScanbotBarcodeScannerSDKInitializer();
            initializer.WithLogging(true, enableNativeLogging: false);
            initializer.UseCameraXRtuUi(false);
            initializer.License(this, ScanbotSDKConfiguration.LICENSE_KEY);
            initializer.Initialize(this);

            operations.ClearStorageDirectory();
        }
    }
}
