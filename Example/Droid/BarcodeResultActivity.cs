
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using IO.Scanbot.Sdk.Barcode.Entity;

namespace BarcodeScannerExample.Droid
{
    [Activity(Theme = "@style/AppTheme")]
    public class BarcodeResultActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.barcode_result);
            
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            string imagePath = null;

            if (BarcodeResultBundle.Instance.PreviewPath != null)
            {
                imagePath = BarcodeResultBundle.Instance.PreviewPath;
            }
            else if (BarcodeResultBundle.Instance.ImagePath != null)
            {
                imagePath = BarcodeResultBundle.Instance.ImagePath;
            }

            if (imagePath != null)
            {
                ShowSnapImage(imagePath);
            }

            ShowBarcodeResult(BarcodeResultBundle.Instance.ScanningResult);
        }

        void ShowSnapImage(string path)
        {
            var items = FindViewById<LinearLayout>(Resource.Id.recognisedItems);

            var view = LayoutInflater.Inflate(Resource.Layout.snap_image_item, items, false);
            items.AddView(view);

            var imageView = view.FindViewById<ImageView>(Resource.Id.snapImage);
            imageView.SetImageURI(Android.Net.Uri.Parse(path));
        }

        void ShowBarcodeResult(BarcodeScanningResult result)
        {
            var parent = FindViewById<LinearLayout>(Resource.Id.recognisedItems);

            foreach (var item in result.BarcodeItems)
            {
                var child = LayoutInflater.Inflate(Resource.Layout.barcode_item, parent, false);

                var image = child.FindViewById<ImageView>(Resource.Id.image);
                var barFormat = child.FindViewById<TextView>(Resource.Id.barcodeFormat);
                var docFormat = child.FindViewById<TextView>(Resource.Id.docFormat);
                var docText = child.FindViewById<TextView>(Resource.Id.docText);

                image.SetImageBitmap(item.Image);
                barFormat.Text = item.BarcodeFormat.Name();
                docFormat.Text = item.BarcodeDocumentFormat?.DocumentFormat;
                docText.Text = item.Text;

                child.Click += delegate
                {
                    BarcodeResultBundle.SelectedBarcodeItem = item;
                    var intent = new Intent(this, typeof(DetailedItemDataActivity));
                    StartActivity(intent);
                };

                parent.AddView(child);
            }
        }
    }
}
