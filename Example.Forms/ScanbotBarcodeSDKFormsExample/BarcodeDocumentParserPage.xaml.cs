using System;
using System.Collections.Generic;
using ScanbotBarcodeSDK.Forms;
using Xamarin.Forms;

namespace ScanbotBarcodeSDKFormsExample
{    
    public partial class BarcodeDocumentParserPage : ContentPage
    {    
        public BarcodeDocumentParserPage ()
        {
            InitializeComponent ();
            parseButton.Clicked += OnParseButtonClicked;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            textEditor.Focus();
        }

        private async void OnParseButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine($"Will parse: {textEditor.Text}");

            BarcodeFormattedResult result = await SBSDK.Operations.ParseBarcodeDocument(textEditor.Text);

            string message;
            if (result != null && result.ParsedSuccessful)
            {
                message = "Parsed successfully!\n";
                message += $"Document Type: {result.DocumentFormat.Value}";
            }
            else
            {
                message = "Cannot perform parsing on the given barcode";
            }

            Utils.Alert(this, "Result", message);
        }
    }
}

