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

		Pharmacy pharmacy;
		IList<Employee> employees = new List<Employee>();
		ListView listView;
		EmployeeAdapter adapter;

		public static EmployeeFragment create(string UUID)
		{
			EmployeeFragment fragment = new EmployeeFragment();
			Bundle arguments = new Bundle();
			arguments.PutString(C_PHARMACY_UUID, UUID);
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
			if (!string.IsNullOrEmpty(pharmacyUUID))
			{
				pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
			}

			listView = view.FindViewById<ListView>(Resource.Id.efEmployeeTable);

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

			if (pharmacy == null)
			{
				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle(Resource.String.error_caption)
						   .SetMessage("Отсутствует аптека!")
						   .SetCancelable(false)
						   .SetPositiveButton(@"OK", (dialog, args) =>
						   {
							   if (dialog is Android.App.Dialog)
							   {
								   ((Android.App.Dialog)dialog).Dismiss();
							   }
						   })
						   .Show();
			}
			else {
				employees = MainDatabase.GetEmployees(pharmacy.UUID);

				adapter = new EmployeeAdapter(Activity, employees);

				listView.Adapter = adapter;
			}
		}

		public override void OnPause()
		{
			base.OnPause();
		}

		public override void OnStop()
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

