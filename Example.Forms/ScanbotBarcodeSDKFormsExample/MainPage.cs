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
            Title = "BARCODE SCANNER";

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
                CreateCell("BATCH BARCODE SCANNER", BatchClicked()),
                CreateCell("PICK IMAGE FROM LIBRARY", ImportButtonClicked()),
                CreateCell("SET ACCEPTED BARCODE TYPES", BarcodeButtonClicked()),
                CreateCell("CLEAR IMAGE STORAGE", StorageCleanupClicked()),
                CreateCell("VIEW LICENSE INFO", ViewLicenseInfoClicked())
            });

            Content = Container;
            Content.BackgroundColor = Color.White;
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
                    FontSize = 12,
                    TextColor = Color.Black
                }
            };

            cell.Tapped += action;

            return cell;
        }

        EventHandler RTUUIClicked()
        {
            return async (sender, e) =>
            {
                if (!Utils.CheckLicense(this))
                {
                    return;
                }
                var config = GetScannerConfiguration(false);
                var result = await SBSDK.Scanner.Open(config);
                if (result.Status == OperationResult.Ok)
                {
                    await Navigation.PushAsync(new BarcodeResultsPage(result.Barcodes));
                }
            };
        }

        EventHandler RTUUIWithImageClicked()
        {
            return async (sender, e) =>
            {
                if (!Utils.CheckLicense(this))
                {
                    return;
                }
                var config = GetScannerConfiguration(true);
                BarcodeResultBundle result = await SBSDK.Scanner.Open(config);
                if (result.Status == OperationResult.Ok)
                {
                    await Navigation.PushAsync(new BarcodeResultsPage(result.Image, result.Barcodes));
                }
            };
        }

        EventHandler BatchClicked()
        {
            return async (sender, e) =>
            {
                var config = new BatchBarcodeScannerConfiguration();
                var result = await SBSDK.Scanner.OpenBatch(config);
                if (result.Status == OperationResult.Ok)
                {
                    await Navigation.PushAsync(new BarcodeResultsPage(result.Barcodes));
                }
            };
        }

        EventHandler ImportButtonClicked()
        {
            return async (sender, e) =>
            {
                if (!Utils.CheckLicense(this))
                {
                    return;
                }
                ImageSource source = await Scanbot.ImagePicker.Forms.ImagePicker.Instance.Pick();
                if (source != null)
                {
                    var barcodes = await SBSDK.Operations.DetectBarcodesFrom(source);
                    await Navigation.PushAsync(new BarcodeResultsPage(source, barcodes));
                }
            };
        }

        EventHandler BarcodeButtonClicked()
        {
            return (sender, e) =>
            {
                if (!Utils.CheckLicense(this))
                {
                    return;
                }
                Navigation.PushAsync(new BarcodeSelectorPage());
            };
        }


        EventHandler StorageCleanupClicked()
        {
            return (sender, e) =>
            {
                if (!Utils.CheckLicense(this))
                {
                    return;
                }

                var result = SBSDK.Operations.ClearStorageDirectory();

                if (result.Status == OperationResult.Ok)
                {
                    Utils.Alert(this, "Success!", "Cleared image storage");
                }
                else
                {
                    Utils.Alert(this, "Oops!", result.Error);
                }
            };
        }

        EventHandler ViewLicenseInfoClicked()
        {
            return (sender, e) =>
            {
                var info = SBSDK.LicenseInfo;
                var message = $"License status {info.Status}";

                if (info.IsValid)
                {
                    message += $" until {info.ExpirationDate.ToString()}";
                }

                Utils.Alert(this, "Info", message);
            };
        }


        BarcodeScannerConfiguration GetScannerConfiguration(bool withImage)
        {
            var configuration = new BarcodeScannerConfiguration();
            configuration.AcceptedFormats = BarcodeTypes.Instance.AcceptedTypes;
            configuration.SuccessBeepEnabled = true;
            //configuration.FinderWidth = 300;
            //configuration.FinderHeight = 200;
            //configuration.FinderTextHint = "Custom hint text...";
            // see further configs...

            if (withImage)
            {
                configuration.BarcodeImageGenerationType = BarcodeImageGenerationType.FromVideoFrame;
            }
            else
            {
                configuration.BarcodeImageGenerationType = BarcodeImageGenerationType.None;
            }

            return configuration;
        }
    }
}
