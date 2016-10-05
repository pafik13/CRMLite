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
		List<Position> Positions;
		LinearLayout EmployeeTable;

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
			Positions = MainDatabase.GetItems<Position>();

			EmployeeTable = view.FindViewById<LinearLayout>(Resource.Id.efEmployeeTable);

			var header = Activity.LayoutInflater.Inflate(Resource.Layout.EmployeeEditTableHeader, EmployeeTable, false);

			EmployeeTable.AddView(header);

			view.FindViewById<Button>(Resource.Id.efAddB).Click += (sender, e) => {
				using (var trans = MainDatabase.BeginTransaction()) {
					var newEmployee = MainDatabase.CreateEmployee(Pharmacy.UUID);
					trans.Commit();

					Employees.Add(newEmployee);
					AddEmployeeToTable(newEmployee);
				}
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
			if (Employees != null && Employees.Count > 0) EmployeeTable.RemoveViews(1, Employees.Count);
			Employees = MainDatabase.GetPharmacyDatas<Employee>(Pharmacy.UUID);
			foreach (var employee in Employees) {
				AddEmployeeToTable(employee);
			}
		}

		public void AddEmployeeToTable(Employee employee)
		{
			var view = Activity.LayoutInflater.Inflate(Resource.Layout.EmployeeEditTableItem, EmployeeTable, false) as LinearLayout;

			view.SetTag(Resource.String.EmployeeUUID, employee.UUID);
			view.SetTag(Resource.String.IsChanged, false);

			var name = view.FindViewById<EditText>(Resource.Id.eetiNameET);
			name.Text = string.IsNullOrEmpty(employee.Name) ? string.Empty : employee.Name;
			name.AfterTextChanged -= ET_AfterTextChanged;
			name.AfterTextChanged += ET_AfterTextChanged;


			/* <Position> */
			var pos = view.FindViewById<Spinner>(Resource.Id.eetiPositionS);
			var positionAdapter = new ArrayAdapter(
				Context, Android.Resource.Layout.SimpleSpinnerItem, Positions.Select(x => x.name).ToArray()
			);
			positionAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			pos.Adapter = positionAdapter;
			pos.ItemSelected -= Pos_ItemSelected;
			pos.ItemSelected += Pos_ItemSelected;
			/* </Position> */
			if (!string.IsNullOrEmpty(employee.Position)) {
				pos.SetSelection(Positions.FindIndex(e => string.Compare(e.uuid, employee.Position) == 0));
			}

			var isCustomer = view.FindViewById<CheckBox>(Resource.Id.eetiIsCustomerCB);
			isCustomer.Checked = employee.IsCustomer;
			isCustomer.CheckedChange -= CB_CheckedChange;
			isCustomer.CheckedChange += CB_CheckedChange;

			var birthDate = view.FindViewById<EditText>(Resource.Id.eetiBirthDateET);
			birthDate.Text = employee.BirthDate.HasValue ? employee.BirthDate.Value.ToString("dd.MM.yy") : string.Empty;
			birthDate.AfterTextChanged -= ET_AfterTextChanged;
			birthDate.AfterTextChanged += ET_AfterTextChanged;

			var phone = view.FindViewById<EditText>(Resource.Id.eetiPhoneET);
			phone.Text = string.IsNullOrEmpty(employee.Phone) ? string.Empty : employee.Phone;
			phone.AfterTextChanged -= ET_AfterTextChanged;
			phone.AfterTextChanged += ET_AfterTextChanged;

			var email = view.FindViewById<EditText>(Resource.Id.eetiEmailET);
			email.Text = string.IsNullOrEmpty(employee.Email) ? string.Empty : employee.Email;
			email.AfterTextChanged -= ET_AfterTextChanged;
			email.AfterTextChanged += ET_AfterTextChanged;

			var canParticipate = view.FindViewById<CheckBox>(Resource.Id.eetiCanParticipateCB);
			canParticipate.Checked = employee.CanParticipate;
			canParticipate.CheckedChange -= CB_CheckedChange;
			canParticipate.CheckedChange += CB_CheckedChange;

			var comment = view.FindViewById<EditText>(Resource.Id.eetiCommentET);
			comment.Text = string.IsNullOrEmpty(employee.Comment) ? string.Empty : employee.Comment;
			comment.AfterTextChanged -= ET_AfterTextChanged;
			comment.AfterTextChanged += ET_AfterTextChanged;

			EmployeeTable.AddView(view);

			//var divider = Activity.LayoutInflater.Inflate(Resource.Layout.Divider, EmployeeTable, false);

			//EmployeeTable.AddView(divider);
		}

		void Pos_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
		{
			if (sender is Spinner) {
				var spinner = ((Spinner)sender);
				var row = (LinearLayout)spinner.Parent;
				row.SetTag(Resource.String.IsChanged, true);
				spinner.SetTag(Resource.String.PositionUUID, Positions[spinner.SelectedItemPosition].uuid);
			}
		}

		void ET_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
		{
			if (sender is EditText) {
				var row = (LinearLayout)((EditText)sender).Parent;
				row.SetTag(Resource.String.IsChanged, true);
			}
		}

		void CB_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			if (sender is CheckBox) {
				var row = (LinearLayout)((CheckBox)sender).Parent.Parent;
				row.SetTag(Resource.String.IsChanged, true);
			}
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
									if (DateTimeOffset.TryParse(birthDate, fmt, DateTimeStyles.AssumeUniversal, out dtoBirthDate)) {
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
			SaveAllEmployees();
		}

		public override void OnStop()
		{
			base.OnStop();
		}
	}
}

