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

				Employee item;
				if (Employee == null) {
					item = MainDatabase.Create<Employee>();
					item.Pharmacy = Pharmacy.UUID;
					item.CreatedAt = DateTimeOffset.Now;
				
				} else {
					item = Employee;
				}
				item.UpdatedAt = DateTimeOffset.Now;

				item.Name = view.FindViewById<EditText>(Resource.Id.edNameET).Text;
				item.Position = Positions[Position.SelectedItemPosition].uuid;
				item.IsCustomer = view.FindViewById<CheckBox>(Resource.Id.edIsCustomerCB).Checked;

				/* BirthDate */
				string birthDate = view.FindViewById<EditText>(Resource.Id.edBirthDateET).Text;
				DateTimeFormatInfo fmt = new CultureInfo("ru-RU").DateTimeFormat;
				if (!string.IsNullOrEmpty(birthDate)) {
					DateTimeOffset dtoBirthDate;
					if (DateTimeOffset.TryParse(birthDate, fmt, DateTimeStyles.None, out dtoBirthDate)) {
						item.BirthDate = dtoBirthDate;
					}
				}
				/* ./BirthDate */

				item.Phone = view.FindViewById<EditText>(Resource.Id.edPhoneET).Text;
				item.Email = view.FindViewById<EditText>(Resource.Id.edEmailET).Text;
				item.CanParticipate = view.FindViewById<CheckBox>(Resource.Id.edCanParticipateCB).Checked;
				item.Comment = view.FindViewById<EditText>(Resource.Id.edCommentET).Text;

				if (!item.IsManaged) MainDatabase.SaveEntity(transaction, item);

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
			}
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

