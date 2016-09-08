using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;
using System.Globalization;

namespace CRMLite.Dialogs
{
	public class EmployeeDialog : DialogFragment
	{
		public const string TAG = @"EmployeeDialog";

		public event EventHandler AfterSaved;

		readonly Pharmacy Pharmacy;
		readonly Employee Employee;

		Spinner Position;
		List<Position> Positions;

		protected virtual void OnAfterSaved(EventArgs e)
		{
			if (AfterSaved != null)
			{
				AfterSaved(this, e);
			}
		}

		public EmployeeDialog(Pharmacy pharmacy, Employee employee = null)
		{
			if (pharmacy == null)
			{
				throw new ArgumentNullException(nameof(pharmacy));
			}

			Pharmacy = pharmacy;
			Employee = employee;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.EmployeeDialog, container, false);

			view.FindViewById<Button>(Resource.Id.edCloseB).Click += (s, e) => {
				Dismiss();
			};

			view.FindViewById<Button>(Resource.Id.edSaveB).Click += (s, e) => {
				var transaction = MainDatabase.BeginTransaction();
				if (Employee == null) {
					var employee = MainDatabase.Create<Employee>();
					employee.Pharmacy = Pharmacy.UUID;
					employee.CreatedAt = DateTimeOffset.Now;
					//employee.UpdatedAt = DateTimeOffset.Now;
					employee.Name = view.FindViewById<EditText>(Resource.Id.edNameET).Text;
					employee.Position = Positions[Position.SelectedItemPosition].uuid;
					employee.IsCustomer = view.FindViewById<CheckBox>(Resource.Id.edIsCustomerCB).Checked;

					string birthDate = view.FindViewById<EditText>(Resource.Id.edBirthDateET).Text;

					DateTimeFormatInfo fmt = new CultureInfo("ru-RU").DateTimeFormat;
					if (!string.IsNullOrEmpty(birthDate)) {
						DateTimeOffset dtoBirthDate;
						if (DateTimeOffset.TryParse(birthDate, fmt, DateTimeStyles.None, out dtoBirthDate)) {
							employee.BirthDate = dtoBirthDate;
						}
					}

					employee.Phone = view.FindViewById<EditText>(Resource.Id.edPhoneET).Text;
					employee.Email = view.FindViewById<EditText>(Resource.Id.edEmailET).Text;
					employee.CanParticipate = view.FindViewById<CheckBox>(Resource.Id.edCanParticipateCB).Checked;
					employee.Comment = view.FindViewById<EditText>(Resource.Id.edCommentET).Text;
				} else {
					//employee.Pharmacy = Pharmacy.UUID;
					//employee.CreatedAt = DateTimeOffset.Now;
					//employee.UpdatedAt = DateTimeOffset.Noww
					Employee.Name = view.FindViewById<EditText>(Resource.Id.edNameET).Text;
					Employee.Position = Positions[Position.SelectedItemPosition].uuid;
					Employee.IsCustomer = view.FindViewById<CheckBox>(Resource.Id.edIsCustomerCB).Checked;

					string birthDate = view.FindViewById<EditText>(Resource.Id.edBirthDateET).Text;

					DateTimeFormatInfo fmt = new CultureInfo("ru-RU").DateTimeFormat;
					if (!string.IsNullOrEmpty(birthDate)) {
						DateTimeOffset dtoBirthDate;
						if (DateTimeOffset.TryParse(birthDate, fmt, DateTimeStyles.None, out dtoBirthDate)) {
							Employee.BirthDate = dtoBirthDate;
						}
					}

					Employee.Phone = view.FindViewById<EditText>(Resource.Id.edPhoneET).Text;
					Employee.Email = view.FindViewById<EditText>(Resource.Id.edEmailET).Text;
					Employee.CanParticipate = view.FindViewById<CheckBox>(Resource.Id.edCanParticipateCB).Checked;
					Employee.Comment = view.FindViewById<EditText>(Resource.Id.edCommentET).Text;

					if (!Employee.IsManaged) MainDatabase.SaveEntity(transaction, Employee);
				}

				transaction.Commit();

				//var sync = new SyncItem()
				//{
				//	Path = @"Employee",
				//	ObjectUUID = employee.UUID,
				//	JSON = JsonConvert.SerializeObject(employee)
				//};

				//MainDatabase.AddToQueue(sync);

				//context.StartService(new Intent("com.xamarin.SyncService"));

				OnAfterSaved(EventArgs.Empty);

				Dismiss();
			};

			Dialog.SetCanceledOnTouchOutside(false);

			/* <Position> */
			Position = view.FindViewById<Spinner>(Resource.Id.edPositionS);
			Positions = MainDatabase.GetItems<Position>();
			var positionAdapter = new ArrayAdapter(
				Activity, Android.Resource.Layout.SimpleSpinnerItem, Positions.Select(x => x.name).ToArray()
			);
			positionAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			Position.Adapter = positionAdapter;
			/* </Position> */


			if (Employee == null){
				Dialog.SetTitle("НОВЫЙ СОТРУДНИК");

				return view;
			}

			Dialog.SetTitle("СОТРУДНИК : " + Employee.Name);

			view.FindViewById<TextView>(Resource.Id.edUUIDTV).Append(Employee.UUID);
			view.FindViewById<EditText>(Resource.Id.edNameET).Append(Employee.Name);
			if (!string.IsNullOrEmpty(Employee.Position)) {
				Position.SetSelection(Positions.FindIndex(item => string.Compare(item.uuid, Employee.Position) == 0));
			};
			view.FindViewById<CheckBox>(Resource.Id.edIsCustomerCB).Checked = Employee.IsCustomer;

			if (Employee.BirthDate.HasValue) {
				view.FindViewById<EditText>(Resource.Id.edBirthDateET).Append(Employee.BirthDate.Value.ToString("dd.MM.yyyy"));
			}

			view.FindViewById<EditText>(Resource.Id.edPhoneET).Append(Employee.Phone);
			view.FindViewById<EditText>(Resource.Id.edEmailET).Append(Employee.Email);
			view.FindViewById<CheckBox>(Resource.Id.edCanParticipateCB).Checked = Employee.CanParticipate;
			view.FindViewById<EditText>(Resource.Id.edCommentET).Append(Employee.Comment);

			return view;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
		}
	}
}

