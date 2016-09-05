using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

using Realms;
using Newtonsoft.Json;
using System.Globalization;

namespace CRMLite.Dialogs
{
	public class EmployeeDialog : DialogFragment
	{

		public event EventHandler AfterSaved;

		Activity context;
		Pharmacy pharmacy;
		Transaction transaction;
		Employee employee;

		Spinner Position;
		List<Position> positions;

		protected virtual void OnAfterSaved(EventArgs e)
		{
			if (AfterSaved != null)
			{
				AfterSaved(this, e);
			}
		}

		public EmployeeDialog(Activity context, Pharmacy pharmacy, Employee employee = null)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}
			if (pharmacy == null)
			{
				throw new ArgumentNullException(nameof(pharmacy));
			}

			this.context = context;
			this.pharmacy = pharmacy;
			this.employee = employee;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			Dialog.SetCanceledOnTouchOutside(false);

			transaction = MainDatabase.BeginTransaction();

			var caption = string.Empty;
			if (employee == null)
			{
				//Dialog.SetTitle("НОВЫЙ СОТРУДНИК");
				caption = "НОВЫЙ СОТРУДНИК";
				employee = MainDatabase.CreateEmployee(pharmacy.UUID);
			} 
			else 
			{
				//Dialog.SetTitle("СОТРУДНИК : " + employee.Name);
				caption = "СОТРУДНИК : " + employee.Name;

				if (employee.LastSyncResult != null)
				{
					caption += string.Format(" (синхр. {0} в {1})"
											 , employee.LastSyncResult.createdAt.ToLocalTime().ToString("dd.MM.yy")
											 , employee.LastSyncResult.createdAt.ToLocalTime().ToString("HH:mm:ss")
											);
				}
			}

			Dialog.SetTitle(caption);

			View view = inflater.Inflate(Resource.Layout.EmployeeDialog, container, false);

			view.FindViewById<TextView>(Resource.Id.edUUIDTV).Text = employee.UUID;
			view.FindViewById<EditText>(Resource.Id.edNameET).Text = employee.Name;

			#region Position
			Position = view.FindViewById<Spinner>(Resource.Id.edPositionS);
			positions = MainDatabase.GetPositions();
			ArrayAdapter positionAdapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, positions.Select(x => x.name).ToArray());
			positionAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			Position.Adapter = positionAdapter;
			Position.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) =>
			{
				employee.Position = positions[e.Position].uuid;
			};
			// SetValue
			if (!string.IsNullOrEmpty(employee.Position))
			{
				Position.SetSelection(positions.FindIndex(item => string.Compare(item.uuid, employee.Position) == 0));
			};
			#endregion

			view.FindViewById<CheckBox>(Resource.Id.edIsCustomerCB).Checked = employee.IsCustomer;

			view.FindViewById<EditText>(Resource.Id.edBirthDateET).Text = employee.BirthDate.HasValue ? string.Empty : employee.BirthDate.Value.ToString("dd.MM.yyyy");

			view.FindViewById<EditText>(Resource.Id.edPhoneET).Text = employee.Phone;
			view.FindViewById<EditText>(Resource.Id.edEmailET).Text = employee.Email;
			view.FindViewById<CheckBox>(Resource.Id.edCanParticipateCB).Checked = employee.CanParticipate;
			view.FindViewById<EditText>(Resource.Id.edCommentET).Text = employee.Comment;

			view.FindViewById<Button>(Resource.Id.edCloseB).Click += delegate {
				if (employee.CreatedAt == null)
				{
					MainDatabase.DeleteEmployee(employee);
				}

				transaction.Commit();

				Dismiss();
			};

			view.FindViewById<Button>(Resource.Id.edSaveB).Click += delegate {
				//Toast.MakeText(context, "SAVE BUTTON CLICKED", ToastLength.Short).Show();
				employee.CreatedAt = DateTimeOffset.Now;
				employee.Name = view.FindViewById<EditText>(Resource.Id.edNameET).Text;
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

				transaction.Commit();

				var sync = new SyncItem()
				{
					Path = @"Employee",
					ObjectUUID = employee.UUID,
					JSON = JsonConvert.SerializeObject(employee)
				};

				MainDatabase.AddToQueue(sync);

				context.StartService(new Intent("com.xamarin.SyncService"));

				OnAfterSaved(EventArgs.Empty);

				Dismiss();
			};

			return view;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			context = null;
			pharmacy = null;
			employee = null;
		}
	}
}

