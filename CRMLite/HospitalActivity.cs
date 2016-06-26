
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

using Realms;

using CRMLite.Entities;
using CRMLite.Adapters;
using CRMLite.Dialogs;

namespace CRMLite
{
	[Activity(Label = "HospitalActivity")]
	public class HospitalActivity : Activity
	{
		Pharmacy pharmacy = null;
		IList<Hospital> hospitals = new List<Hospital>();
		ListView listView = null;
		HospitalAdapter adapter = null;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Hospital);

			var pharmacyUUID = Intent.GetStringExtra("UUID");
			if (!string.IsNullOrEmpty(pharmacyUUID))
			{
				pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
				FindViewById<TextView>(Resource.Id.haInfoTV).Text = "ЛПУ БЛИЗКИЕ К АПТЕКЕ : " + pharmacy.LegalName;
			}

			listView = FindViewById<ListView>(Resource.Id.haHospitalTable);

			listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
				HospitalDialog hospitalDialog = new HospitalDialog(this, pharmacy, hospitals[e.Position]);
				hospitalDialog.Show(fragmentTransaction, "HospitalDialog");
				hospitalDialog.AfterSaved += delegate
				{
					Console.WriteLine("Event {0} was called", "AfterSaved");
						if (listView.Adapter is HospitalAdapter)
					{
						((HospitalAdapter)listView.Adapter).NotifyDataSetChanged();
					}
				};
			};

			FindViewById<Button>(Resource.Id.haCloseB).Click += delegate
			{
				Finish();
			};

			FindViewById<ImageView>(Resource.Id.haAdd).Click += delegate
			{
				//Toast.MakeText(this, "ADD BUTTON CLICKED", ToastLength.Short).Show();
				Console.WriteLine("Event {0} was called", "haAdd_Click");
				FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
				HospitalDialog hospitalDialog = new HospitalDialog(this, pharmacy);
				hospitalDialog.Show(fragmentTransaction, "HospitalDialog");
				hospitalDialog.AfterSaved += (object sender, EventArgs e) =>
				{
					Console.WriteLine("Event {0} was called", "AfterSaved");
					hospitals = MainDatabase.GetHospitals(pharmacy.UUID);

					adapter = new HospitalAdapter(this, hospitals);

					listView.Adapter = adapter;
				};
			};
		}

		protected override void OnResume()
		{
			base.OnResume();

			if (pharmacy == null)
			{
				new AlertDialog.Builder(this)
							   .SetTitle(Resource.String.error_caption)
							   .SetMessage("Отсутствует аптека!")
							   .SetCancelable(false)
							   .SetPositiveButton(@"OK", (dialog, args) =>
							   {
								   if (dialog is Dialog)
								   {
									   ((Dialog)dialog).Dismiss();
									   Finish();
								   }
							   })
							   .Show();
			}
			else {

				Transaction transaction = MainDatabase.BeginTransaction();

				hospitals = MainDatabase.GetHospitals(pharmacy.UUID);

				foreach (var item in hospitals)
				{
					item.LastSyncResult = MainDatabase.GetSyncResult(item.UUID);
				}

				transaction.Commit();

				adapter = new HospitalAdapter(this, hospitals);

				listView.Adapter = adapter;
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
		}

		protected override void OnStop()
		{
			base.OnStop();

			if (pharmacy != null)
			{
				listView.Adapter = null;
				adapter = null;
			}

		}
	}
}


