using System;
using System.Collections.Generic;
using System.Linq;

using Android.OS;
using Android.Views;
using Android.Widget;

using Android.Support.V4.App;

using CRMLite.Dialogs;
using CRMLite.Entities;
using CRMLite.Adapters;

namespace CRMLite
{
	public class EmployeeFragment : Fragment
	{
		public const string C_PHARMACY_UUID = @"C_PHARMACY_UUID";

		Pharmacy Pharmacy;
		IList<Employee> Employees;
		ListView EmployeeTable;

		public static EmployeeFragment create(string pharmacyUUID)
		{
			var fragment = new EmployeeFragment();
			var arguments = new Bundle();
			arguments.PutString(C_PHARMACY_UUID, pharmacyUUID);
			fragment.Arguments = arguments;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);

			View view = inflater.Inflate(Resource.Layout.EmployeeFragment, container, false);

			var pharmacyUUID = Arguments.GetString(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return view;

			Pharmacy = MainDatabase.GetEntity<Pharmacy>(pharmacyUUID);

			EmployeeTable = view.FindViewById<ListView>(Resource.Id.efEmployeeTable);

			//listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			//{
			//	FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
			//	EmployeeDialog employeeDialog = new EmployeeDialog(this, pharmacy, employees[e.Position]);
			//	employeeDialog.Show(fragmentTransaction, "EmployeeDialog");
			//	employeeDialog.AfterSaved += delegate
			//	{
			//		Console.WriteLine("Event {0} was called", "AfterSaved");
			//		//employees = MainDatabase.GetEmployees(pharmacy.UUID);

			//		//adapter = new EmployeeAdapter(this, employees);

			//		//listView.Adapter = adapter;

			//		if (listView.Adapter is EmployeeAdapter)
			//		{
			//			((EmployeeAdapter)listView.Adapter).NotifyDataSetChanged();
			//		}
			//	};
			//};


			return view;

		}

		public override void OnResume()
		{
			base.OnResume();

			if (Pharmacy == null) {
				new Android.App.AlertDialog.Builder(Context)
						   .SetTitle(Resource.String.error_caption)
						   .SetMessage("Отсутствует аптека!")
						   .SetCancelable(false)
						   .SetPositiveButton(@"OK", (dialog, args) => {
							   if (dialog is Android.App.Dialog) {
								   ((Android.App.Dialog)dialog).Dismiss();
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

			EmployeeTable.Adapter = new EmployeeAdapter(Activity, Employees);
		}


		public override void OnPause()
		{
			base.OnPause();
		}

		public override void OnStop()
		{
			base.OnStop();
		}
	}
}

