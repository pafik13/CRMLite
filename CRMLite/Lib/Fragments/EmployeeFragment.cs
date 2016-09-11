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
using System.Globalization;

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

			view.FindViewById<Button>(Resource.Id.efAddB).Click += (sender, e) => {
				using (var trans = MainDatabase.BeginTransaction()) {
					Employees.Add(MainDatabase.CreateEmployee(Pharmacy.UUID));
					trans.Commit();
				}
				(EmployeeTable.Adapter as EmployeeEditAdapter).NotifyDataSetChanged();
			}; 

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

			EmployeeTable.Adapter = new EmployeeEditAdapter(Activity, Employees);
		}

		public void SaveAllEmployees()
		{
			using (var trans = MainDatabase.BeginTransaction()) {
				for (int c = 0; c < EmployeeTable.ChildCount; c++) {
					var row = EmployeeTable.GetChildAt(c);
					if (row is LinearLayout) {
						row = (LinearLayout)row;
						var isChanged = (bool)row.GetTag(Resource.String.IsChanged);
						if (isChanged) {
							var employeeUUID = (string)row.GetTag(Resource.String.EmployeeUUID);
							if (!string.IsNullOrEmpty(employeeUUID)) {
								var employee = MainDatabase.GetEntity<Employee>(employeeUUID);
								employee.UpdatedAt = DateTimeOffset.Now;

								employee.Name = row.FindViewById<EditText>(Resource.Id.eetiNameET).Text;

								var positionUUID = (string)row.FindViewById<Spinner>(Resource.Id.eetiPositionS).GetTag(Resource.String.PositionUUID);
								if (!string.IsNullOrEmpty(positionUUID)) {
									employee.Position = positionUUID;
								}

								employee.IsCustomer = row.FindViewById<CheckBox>(Resource.Id.eetiIsCustomerCB).Checked;

								/* BirthDate */
								string birthDate = row.FindViewById<EditText>(Resource.Id.eetiBirthDateET).Text;
								DateTimeFormatInfo fmt = new CultureInfo("ru-RU").DateTimeFormat;
								if (!string.IsNullOrEmpty(birthDate)) {
									DateTimeOffset dtoBirthDate;
									if (DateTimeOffset.TryParse(birthDate, fmt, DateTimeStyles.None, out dtoBirthDate)) {
										employee.BirthDate = dtoBirthDate;
									}
								}
								/* ./BirthDate */

								employee.Phone = row.FindViewById<EditText>(Resource.Id.eetiPhoneET).Text;
								employee.Email = row.FindViewById<EditText>(Resource.Id.eetiEmailET).Text;
								employee.CanParticipate = row.FindViewById<CheckBox>(Resource.Id.eetiCanParticipateCB).Checked;
								employee.Comment = row.FindViewById<EditText>(Resource.Id.eetiCommentET).Text;

								if (!employee.IsManaged) MainDatabase.SaveEntity(trans, employee);
							}
						}
					}
				}
				trans.Commit();
			}
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

