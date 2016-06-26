using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

using Realms;
using Newtonsoft.Json;

namespace CRMLite.Dialogs
{
	public class EmployeeDialog : DialogFragment
	{

		public event EventHandler AfterSaved;

		Activity context = null;
		Pharmacy pharmacy = null;
		//ListView listView = null;
		Transaction transaction = null;
		Employee employee = null;

		protected virtual void OnAfterSaved(EventArgs e)
		{
			if (AfterSaved != null)
			{
				AfterSaved(this, e);
			}
		}

		public EmployeeDialog(Activity context, Pharmacy pharmacy, /*ListView listView, */ Employee employee = null)
		{
			if (context == null || pharmacy == null /*|| listView == null*/)
			{
				throw new ArgumentNullException();
			}

			this.context = context;
			this.pharmacy = pharmacy;
			this.employee = employee;
			//this.listView = listView;
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

			view.FindViewById<EditText>(Resource.Id.edNameET).Text = employee.Name;
			view.FindViewById<EditText>(Resource.Id.edPositionET).Text = employee.Position;
			//if (employee.BirthDate != null)
			//{
			//	view.FindViewById<DatePicker>(Resource.Id.edBirthDateDP).DateTime = employee.BirthDate.Value.DateTime;
			//}
			view.FindViewById<EditText>(Resource.Id.edPhoneET).Text = employee.Phone;
			view.FindViewById<EditText>(Resource.Id.edLoyaltyET).Text = employee.Loyalty;
			view.FindViewById<CheckBox>(Resource.Id.edIsCustomerCB).Checked = employee.IsCustomer;


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
				employee.Position = view.FindViewById<EditText>(Resource.Id.edPositionET).Text;
				//if (view.FindViewById<DatePicker>(Resource.Id.edBirthDateDP).DateTime != DateTime.MinValue)
				//{
				//	employee.BirthDate = view.FindViewById<DatePicker>(Resource.Id.edBirthDateDP).DateTime;
				//}
				employee.Phone = view.FindViewById<EditText>(Resource.Id.edPhoneET).Text;
				employee.Loyalty = view.FindViewById<EditText>(Resource.Id.edLoyaltyET).Text;
				employee.IsCustomer = view.FindViewById<CheckBox>(Resource.Id.edIsCustomerCB).Checked;

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
			this.context = null;
			this.pharmacy = null;
			this.employee = null;
			//this.listView = null;
		}
	}
}

