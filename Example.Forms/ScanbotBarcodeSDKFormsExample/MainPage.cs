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
                    var isValid = Utils.CheckLicense(this);
                    if (!isValid)
                    {
                        return;
                    }

                    await Navigation.PushAsync(new BarcodeResultsPage(result.Barcodes));
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
                    var isValid = Utils.CheckLicense(this);
                    if (!isValid)
                    {
                        return;
                    }

                    var source = ImageSource.FromFile(result.ImagePath);
                    await Navigation.PushAsync(new BarcodeResultsPage(source, result.Barcodes));
                }
            };
        }

        EventHandler ImportButtonClicked()
        {
            return async (sender, e) =>
            {
                ImageSource source = await Scanbot.ImagePicker.Forms.ImagePicker.Instance.Pick();
                if (source != null)
                {
                    var isValid = Utils.CheckLicense(this);
                    if (!isValid)
                    {
                        return;
                    }

                    await Navigation.PushAsync(new BarcodeResultsPage(source));
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
            //configuration.AcceptedFormats = BarcodeTypes.Instance.AcceptedTypes;

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
