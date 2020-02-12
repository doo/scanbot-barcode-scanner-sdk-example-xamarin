using System;
using System.Collections.Generic;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Sdk.Barcode.Entity;

namespace BarcodeScannerExample.Droid
{
    public class BarcodeTypesAdapter : RecyclerView.Adapter
    {
        public List<BarcodeFormat> Items = BarcodeFormat.Values().ToList();

        public override int ItemCount => Items.Count;

        BarcodeFormat LastCheckedFormat;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            LastCheckedFormat = Items[position];

            var barcodeHolder = (BarcodeViewHolder)holder;
            barcodeHolder.Name.Text = LastCheckedFormat.Name();
            barcodeHolder.Checker.Checked = BarcodeTypes.Instance.IsChecked(LastCheckedFormat);

            barcodeHolder.Checker.CheckedChange += OnCheckedChange;
        }

        void OnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            BarcodeTypes.Instance.Update(LastCheckedFormat, e.IsChecked);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var inflater = LayoutInflater.From(parent.Context);
            var view = inflater.Inflate(Resource.Layout.barcode_type, parent, false);
            return new BarcodeViewHolder(view);
        }
    }

    class BarcodeViewHolder : RecyclerView.ViewHolder
    {
        public TextView Name { get; private set; }

        public CheckBox Checker { get; private set; }

        public BarcodeViewHolder(View item) : base(item)
        {
            Name = item.FindViewById<TextView>(Resource.Id.barcode_type_name);
            Checker = item.FindViewById<CheckBox>(Resource.Id.barcode_type_checker);
        }
    }
}
