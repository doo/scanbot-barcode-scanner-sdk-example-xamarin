using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScanbotBarcodeSDK.Forms;
using Xamarin.Forms;

namespace ScanbotBarcodeSDKFormsExample
{
    public class MainPage : ContentPage
    {
        StackLayout Container { get; set; }

        Image BarcodeImage { get; set; }

        private bool ShouldTestCloseView = false;

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
                CreateCell("RTU UI - BARCODE SCANNER", RTUUIClicked()),
                CreateCell("RTU UI WITH BARCODE IMAGE", RTUUIWithImageClicked()),
                CreateCell("RTU UI - BATCH BARCODE SCANNER", BatchClicked()),
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
                TestCloseView(false);
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
                TestCloseView(false);
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
                var configuration = new BatchBarcodeScannerConfiguration();
                configuration.AcceptedFormats = BarcodeTypes.Instance.AcceptedTypes;
                configuration.OverlayConfiguration = new SelectionOverlayConfiguration(Color.Yellow, Color.Yellow, Color.Black);
                TestCloseView(true);
                var result = await SBSDK.Scanner.OpenBatch(configuration);
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
                    List<Barcode> codes = null;
                    try
                    {
                        codes = await SBSDK.Operations.DetectBarcodesFrom(source);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("No barcodes found: " + exception.Message);
                        codes = new List<Barcode>();
                    }
                    await Navigation.PushAsync(new BarcodeResultsPage(source, codes));
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
            var configuration = new BarcodeScannerConfiguration
            {
                AcceptedFormats = BarcodeTypes.Instance.AcceptedTypes,
                SuccessBeepEnabled = true,
                
            };

            configuration.OverlayConfiguration = new SelectionOverlayConfiguration(Color.Yellow, Color.Yellow, Color.Black, Color.Pink, Color.Red, Color.PeachPuff);

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

        /// <summary>
        /// Test the force closing of Barcode scanning view.
        /// </summary>
        /// <param name="isBatchBarcode"></param>
        /// <returns></returns>
        private void TestCloseView(bool isBatchBarcode)
        {
            if (!ShouldTestCloseView) return;
            Task.Run(async () =>
            {
                await Task.Delay(7000);
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    if (isBatchBarcode)
                    {
                        SBSDK.Scanner.CloseBatchBarcodeScannerView();
                    }
                    else
                    {
                        SBSDK.Scanner.CloseBarcodeScannerView();
                    }
                });
            });
        }
    }
}
