﻿using System;
using ScanbotBarcodeSDK.Forms;
using Xamarin.Forms;

namespace NativeBarcodeSDKRenderer.Views
{
    /*
       This is the View that will be referenced in the XAML to integrate our native Camera View.
       The class itself does not provide any specific implementation; the actual implementation
       is achieved through the use of Custom Renderers.
       Renderers are platform specific and they are implemented in the relative native projects.
       Take a look at AndroidBarcodeCameraRenderer.cs to see how the implementation is done.
    */
    public class BarcodeCameraView : View
    {
        // This is the delegate that will be used from our native controller to
        // notify us that the scanner has returned a valid result.
        // We can set this from our Page class to implement a custom behavior.
        public delegate void BarcodeScannerResultHandler(BarcodeResultBundle result);
        public BarcodeScannerResultHandler OnBarcodeScanResult;

        // This event is defined from our native control through the Custom Renderer.
        // We call this from our Page in accord to the view lifecycle (OnAppearing)
        public EventHandler<EventArgs> OnResumeHandler;
        public void Resume()
        {
            OnResumeHandler?.Invoke(this, EventArgs.Empty);
        }

        // This event is defined from our native control through the Custom Renderer.
        // We call this from our Page in accord to the view lifecycle (OnDisappearing)
        public EventHandler<EventArgs> OnPauseHandler;
        public void Pause()
        {
            OnPauseHandler?.Invoke(this, EventArgs.Empty);
        }

        // This event is defined from our native control through the Custom Renderer.
        // We call this from our Page when we want to start detecting barcodes.
        public EventHandler<EventArgs> StartDetectionHandler;
        public void StartDetection()
        {
            StartDetectionHandler?.Invoke(this, EventArgs.Empty);
        }

        // This event is defined from our native control through the Custom Renderer.
        // We call this from our Page when we want to stop detecting barcodes.
        public EventHandler<EventArgs> StopDetectionHandler;
        public void StopDetection()
        {
            StopDetectionHandler?.Invoke(this, EventArgs.Empty);
        }

        public BarcodeCameraView()
        {
            OnBarcodeScanResult = HandleBarcodeScanResult;
        }

        /// <summary>
        /// BarcodeResultBundle 
        /// </summary>
        /// <param name="result"></param>
        private void HandleBarcodeScanResult(BarcodeResultBundle result)
        {
            // If we don't implement the delegate from our Page class, this method
            // will be called instead as a fallback mechanism.
        }

        /// <summary>
        /// Toggle Flash Binding property
        /// </summary>
        public static readonly BindableProperty ToggleFlashProperty =
          BindableProperty.Create("ToggleFlash", typeof(bool), typeof(BarcodeCameraView), false);

        /// <summary>
        /// Toggle Flash property.
        /// </summary>
        public bool ToggleFlash
        {
            get { return (bool)GetValue(ToggleFlashProperty); }
            set { SetValue(ToggleFlashProperty, value); }
        }
    }
}

