
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
using CRMLite.Dialogs;

namespace CRMLite
{
	[Activity(Label = "EmploeeActivity")]
	public class EmploeeActivity : Activity
	{
		Pharmacy pharmacy = null;
		IList<Employee> employees = new List<Employee>();
		ListView listView = null;
		EmploeeAdapter adapter = null;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Emploee);

			var pharmacyUUID = Intent.GetStringExtra("UUID");
			if (!string.IsNullOrEmpty(pharmacyUUID)) {
				pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
				FindViewById<TextView>(Resource.Id.eaInfoTV).Text = "СОТРУДНИКИ АПТЕКИ : " + pharmacy.LegalName;
			}

			listView = FindViewById<ListView>(Resource.Id.eaEmploeeTable);

			listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
				EmploeeDialog emploeeDialog = new EmploeeDialog(this, pharmacy, employees[e.Position]);
				emploeeDialog.Show(fragmentTransaction, "EmploeeDialog");
				emploeeDialog.AfterSaved += delegate 
				{
					Console.WriteLine("Event {0} was called", "AfterSaved");
					//employees = MainDatabase.GetEmployees(pharmacy.UUID);

					//adapter = new EmploeeAdapter(this, employees);

					//listView.Adapter = adapter;

					if (listView.Adapter is EmploeeAdapter)
					{
						((EmploeeAdapter)listView.Adapter).NotifyDataSetChanged();
					}
				};
			};

			FindViewById<Button>(Resource.Id.eaCloseB).Click += delegate {
				Finish();
			};

			FindViewById<ImageView>(Resource.Id.eaAdd).Click += delegate
			{
				//Toast.MakeText(this, "ADD BUTTON CLICKED", ToastLength.Short).Show();
				Console.WriteLine("Event {0} was called", "eaAdd_Click");
				FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
				EmploeeDialog emploeeDialog = new EmploeeDialog(this, pharmacy);
				emploeeDialog.Show(fragmentTransaction, "EmploeeDialog");
				emploeeDialog.AfterSaved += (object sender, EventArgs e) =>
				{
					Console.WriteLine("Event {0} was called", "AfterSaved");
					employees = MainDatabase.GetEmployees(pharmacy.UUID);

					adapter = new EmploeeAdapter(this, employees);

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

				employees = MainDatabase.GetEmployees(pharmacy.UUID);

				adapter = new EmploeeAdapter(this, employees);

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

