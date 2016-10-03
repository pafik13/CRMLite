using System;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Content.PM;

using CRMLite.Entities;
using CRMLite.Adapters;
using CRMLite.Dialogs;

namespace CRMLite
{
	[Activity(Label = "EmployeeActivity", ScreenOrientation = ScreenOrientation.Landscape)]
	public class EmployeeActivity : Activity
	{
		public const string C_PHARMACY_UUID = @"C_PHARMACY_UUID";

		Pharmacy Pharmacy;
		IList<Employee> Employees;
		ListView EmployeeTable;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Employee);

			FindViewById<Button>(Resource.Id.eaCloseB).Click += (s, e) => {
				Finish();
			};

			var pharmacyUUID = Intent.GetStringExtra(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return;

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
			FindViewById<TextView>(Resource.Id.eaInfoTV).Text = "СОТРУДНИКИ АПТЕКИ : " + Pharmacy.GetName();

			EmployeeTable = FindViewById<ListView>(Resource.Id.eaEmployeeTable);

			var header = LayoutInflater.Inflate(Resource.Layout.EmployeeTableHeader, EmployeeTable, false);
			EmployeeTable.AddHeaderView(header);

			EmployeeTable.ItemClick += (sender, e) => {
				Employee item;
				if (EmployeeTable.ChildCount == Employees.Count + 1) {
					item = Employees[e.Position - 1];
				} else {
					item = Employees[e.Position];
				}

				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(EmployeeDialog.TAG);
				if (prev != null) {
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var employeeDialog = new EmployeeDialog(Pharmacy, item);
				employeeDialog.Show(fragmentTransaction, EmployeeDialog.TAG);
				employeeDialog.AfterSaved += (caller, arguments) => {
					Console.WriteLine("Event {0} was called", "AfterSaved");

					RecreateAdapter();
				};
			};

			FindViewById<ImageView>(Resource.Id.eaAddIV).Click += (s, e) => {
				Console.WriteLine("Event {0} was called", "eaAdd_Click");

				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(EmployeeDialog.TAG);
				if (prev != null) {
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var employeeDialog = new EmployeeDialog(Pharmacy);
				employeeDialog.Show(fragmentTransaction, EmployeeDialog.TAG);
				employeeDialog.AfterSaved += (caller, arguments) => {
					Console.WriteLine("Event {0} was called", "AfterSaved");

					RecreateAdapter();
				};
			};
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
									   }
								   })
								   .Show();

			} else {
				RecreateAdapter();
			}
		}

		void RecreateAdapter()
		{
			Employees = MainDatabase.GetPharmacyDatas<Employee>(Pharmacy.UUID);

			EmployeeTable.Adapter = new EmployeeAdapter(this, Employees);
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

