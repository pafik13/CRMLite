using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;
using CRMLite.Adapters;

namespace CRMLite
{
	[Activity(Label = "ContractActivity", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape)]
	public class ContractActivity : Activity
	{
		public const string C_PHARMACY_UUID = @"C_PHARMACY_UUID";

		Pharmacy Pharmacy;
		ListView ContractTable;
		IList<ContractData> ContractDatas;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Contract);

			FindViewById<Button>(Resource.Id.caCloseB).Click += (s, e) => {
				Finish();
			};

			var pharmacyUUID = Intent.GetStringExtra(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return;

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);

			FindViewById<TextView>(Resource.Id.caInfoTV).Text = "КОНТРАКТЫ АПТЕКИ: " + Pharmacy.GetName();

			ContractTable = FindViewById<ListView>(Resource.Id.caContractTable);

			var header = LayoutInflater.Inflate(Resource.Layout.ContractTableHeader, ContractTable, false);
			ContractTable.AddHeaderView(header);
		}

		protected override void OnResume()
		{
			base.OnResume();

			if (Pharmacy == null) {
				new AlertDialog.Builder(this)
							   .SetTitle(Resource.String.error_caption)
							   .SetMessage("Отсутствует аптека!")
							   .SetCancelable(false)
							   .SetPositiveButton(@"OK", (dialog, args) => {
								   if (dialog is Dialog) {
									   ((Dialog)dialog).Dismiss();
									   Finish();
								   }
							   })
							   .Show();
			} else {

				RecreateAdapter();
			}
		}

		void RecreateAdapter()
		{
			ContractDatas = MainDatabase.GetPharmacyDatas<ContractData>(Pharmacy.UUID);

			if (ContractDatas.Count == 0) return;

			ContractTable.Adapter = new ContractAdapter(this, ContractDatas);
		}

		protected override void OnPause()
		{
			base.OnPause();
		}

		protected override void OnStop()
		{
			base.OnStop();
		}
	}
}

