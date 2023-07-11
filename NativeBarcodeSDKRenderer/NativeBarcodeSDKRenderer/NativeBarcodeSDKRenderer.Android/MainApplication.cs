using System;
using Android.App;
using Android.Runtime;
using ScanbotBarcodeSDK.Forms.Droid;

namespace NativeBarcodeSDKRenderer.Droid
{
#if DEBUG
    [Application(Debuggable = true, LargeHeap = true, Theme = "@style/MainTheme")]
#else
    [Application(Debuggable = false, LargeHeap = true, Theme = "@style/MainTheme")]
#endif
    public class MainApplication : Application
    {

        static readonly string LOG_TAG = nameof(MainApplication);

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
            operations.ClearStorageDirectory();
        }
    }
}
