
using System.Text;
using Android.App;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.V7.App;
using Android.Widget;
using IO.Scanbot.Barcodescanner.Model.BoardingPass;
using IO.Scanbot.Barcodescanner.Model.DEMedicalPlan;
using IO.Scanbot.Barcodescanner.Model.DisabilityCertificate;
using IO.Scanbot.Barcodescanner.Model.SEPA;
using IO.Scanbot.Barcodescanner.Model.VCard;
using IO.Scanbot.Sdk.Barcode.Entity;

namespace BarcodeScannerExample.Droid
{
    [Activity(Theme = "@style/AppTheme")]
    public class DetailedItemDataActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.detailed_item_data);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            var item = BarcodeResultBundle.SelectedBarcodeItem;

            var container = FindViewById<ConstraintLayout>(Resource.Id.container);

            container.FindViewById<ImageView>(Resource.Id.image)
                .SetImageBitmap(item.Image);
            container.FindViewById<TextView>(Resource.Id.barcodeFormat)
                .Text = item.BarcodeFormat.Name();
            container.FindViewById<TextView>(Resource.Id.docFormat)
                .Text = item.BarcodeDocumentFormat?.DocumentFormat;
            container.FindViewById<TextView>(Resource.Id.description)
                .Text = ParseFormat(item);
        }

        private string ParseFormat(BarcodeItem item)
        {
            if (item.BarcodeDocumentFormat == null)
            {
                return item.Text;
            }

            var format = item.BarcodeDocumentFormat;

            var result = new StringBuilder();
            result.Append("\n");

            if (format is BoardingPassDocument)
            {
                var boardingPass = (BoardingPassDocument)format;
                result.Append("Boarding Pass Document").Append("\n");
                result.Append(boardingPass.Name).Append("\n");

                foreach (BoardingPassLeg leg in boardingPass.Legs)
                {
                    foreach (BoardingPassLegField field in leg.Fields)
                    {
                        result.Append(field.Type.Name()).Append(": ").Append(field.Value).Append("\n");
                    }
                }
            }
            else if (format is DEMedicalPlanDocument)
            {
                var medical = (DEMedicalPlanDocument)format;
                result.Append("DE Medical Plan Document").Append("\n");

                result.Append("Doctor Fields: ").Append("\n");
                foreach (DEMedicalPlanDoctorField field in medical.Doctor.Fields)
                {
                    result.Append(field.Type.Name()).Append(": ").Append(field.Value).Append("\n");
                }
                result.Append("\n");

                result.Append("Patient Fields: ").Append("\n");
                foreach (DEMedicalPlanPatientField field in medical.Patient.Fields)
                {
                    result.Append(field.Type.Name()).Append(": ").Append(field.Value).Append("\n");
                }
                result.Append("\n");

                result.Append("Medicine Fields: ").Append("\n");
                foreach (DEMedicalPlanStandardSubheading heading in medical.Subheadings)
                {
                    foreach (DEMedicalPlanMedicine medicine in heading.Medicines)
                    {
                        foreach (DEMedicalPlanMedicineField field in medicine.Fields)
                        {
                            result.Append(field.Type.Name()).Append(": ").Append(field.Value).Append("\n");
                        }
                    }
                }
                result.Append("\n");
            }
            else if (format is DisabilityCertificateDocument)
            {
                result.Append("Disability Certificate Document").Append("\n");

                foreach (DisabilityCertificateDocumentField field in ((DisabilityCertificateDocument)format).Fields)
                {
                    result.Append(field.Type.Name()).Append(": ").Append(field.Value).Append("\n");
                }
            }
            else if (format is SEPADocument)
            {
                result.Append("SEPA Document").Append("\n");

                foreach (SEPADocumentField field in ((SEPADocument)format).Fields)
                {
                    result.Append(field.Type.Name()).Append(": ").Append(field.Value).Append("\n");
                }
            }
            else if (format is VCardDocument)
            {
                result.Append("VCard Document").Append("\n");

                foreach (VCardDocumentField field in ((VCardDocument)format).Fields)
                {
                    result.Append(field.Type.ToString()).Append(": ").Append(field.RawText).Append("\n");
                }
            }

            return result.ToString();
        }
    }
}
