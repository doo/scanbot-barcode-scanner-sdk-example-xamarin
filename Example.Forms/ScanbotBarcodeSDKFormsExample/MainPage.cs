using System;
using ScanbotBarcodeSDK.Forms;
using Xamarin.Forms;

namespace ScanbotBarcodeSDKFormsExample
{
    public class MainPage : ContentPage
    {
        StackLayout Container { get; set; }

        Image BarcodeImage { get; set; }

        public MainPage()
        {
            Container = new StackLayout();
            Container.Orientation = StackOrientation.Vertical;
            Container.BackgroundColor = Color.White;

            BarcodeImage = new Image();
            BarcodeImage.HeightRequest = Container.Width / 2;
            Container.Children.Add(BarcodeImage);

            var table = new TableView();
            table.BackgroundColor = Color.White;
            Container.Children.Add(table);

            table.Root = new TableRoot();
            table.Root.Add(new TableSection("EXAMPLES")
            {
                CreateCell("READY-TO-USE UI", RTUUIClicked()),
                CreateCell("RTU UI WITH BARCODE IMAGE", RTUUIWithImageClicked()),
                CreateCell("PICK IMAGE FROM LIBRARY", ImportButtonClicked()),
                CreateCell("SET ACCEPTED BARCODE TYPES", BarcodeButtonClicked())
            });

            Content = Container;
            Content.BackgroundColor = Color.White;
        }

        void ShowImage(ImageSource source)
        {
            BarcodeImage.Source = source;
            BarcodeImage.WidthRequest = Container.Width;
            BarcodeImage.HeightRequest = Container.Width / 2;
            BarcodeImage.Aspect = Aspect.AspectFit;
        }

        ViewCell CreateCell(string title, EventHandler action)
        {
            var cell = new ViewCell
            {
                View = new Label
                {
                    Text = title,
                    VerticalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    FontSize = 12
                }
            };

            cell.Tapped += action;

            return cell;
        }

        EventHandler RTUUIClicked()
        {
            return async (sender, e) =>
            {
                var config = GetScannerConfiguration(false);
                var result = await SBSDK.Scanner.Open(config);
                if (result.Status == OperationResult.Ok)
                {
                    if (result.Barcodes.Count == 0)
                    {
                        return;
                    }
                    var code = result.Barcodes[0];
                    await DisplayAlert("Barcode", $"Format: {code.Format}, value:\n\n{code.Text}", "Close");
                }
            };
        }

        EventHandler RTUUIWithImageClicked()
        {
            return async (sender, e) =>
            {
                var config = GetScannerConfiguration(true);
                var result = await SBSDK.Scanner.Open(config);
                if (result.Status == OperationResult.Ok)
                {
                    if (result.Barcodes.Count == 0)
                    {
                        return;
                    }
                    var code = result.Barcodes[0];
                    ShowImage(code.Image);
                    await DisplayAlert("Barcode", $"Format: {code.Format}, value:\n\n{code.Text}", "Close");
                }
            };
        }

        EventHandler ImportButtonClicked()
        {
            return async (sender, e) =>
            {
                var result = await SBSDK.ImagePicker.PickImageAsync();
                if (result != null)
                {
                    ShowImage(result);
                }
            };
        }

        EventHandler BarcodeButtonClicked()
        {
            return (sender, e) =>
            {
                Navigation.PushAsync(new BarcodeSelectorPage());
            };
        }

        BarcodeScannerConfiguration GetScannerConfiguration(bool withImage)
        {
            var configuration = new BarcodeScannerConfiguration();

            if (withImage)
            {
                configuration.BarcodeImageGenerationType = BarcodeImageGenerationType.CapturedImage;
            }
            else
            {
                configuration.BarcodeImageGenerationType = BarcodeImageGenerationType.None;
            }

            return configuration;
        }
    }
}
