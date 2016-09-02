
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;
using CRMLite.Adapters;

namespace CRMLite
{
	[Activity(Label = "ContractActivity")]
	public class ContractActivity : Activity
	{
		Pharmacy Pharmacy;
		ListView ContractTable;
		List<ContractData> Datas;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);

			// Create your application here
			var pharmacyUUID = Intent.GetStringExtra("UUID");
			if (string.IsNullOrEmpty(pharmacyUUID)) return;

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);

			FindViewById<TextView>(Resource.Id.caInfoTV).Text = "КОНТРАКТЫ АПТЕКИ: " + Pharmacy.GetName();

			ContractTable = FindViewById<ListView>(Resource.Id.caContractTable);
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
				Datas = new List<ContractData>();//MainDatabase.(Pharmacy.UUID);

				ContractTable.Adapter = new ContractAdapter(this, Datas ?? new List<ContractData>());
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
		}

		protected override void OnStop()
		{
			base.OnStop();

			if (Pharmacy != null) {
				ContractTable.Adapter = null;
				Datas = null;
			}

		}
	}
}

