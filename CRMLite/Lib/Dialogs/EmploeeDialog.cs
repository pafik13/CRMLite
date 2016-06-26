
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;
using CRMLite.Adapters;

using Realms;

namespace CRMLite.Dialogs
{
	public class EmploeeDialog : DialogFragment
	{

		public event EventHandler AfterSaved;

		Activity context = null;
		Pharmacy pharmacy = null;
		//ListView listView = null;
		Transaction transaction = null;
		Employee emploee = null;

		protected virtual void OnAfterSaved(EventArgs e)
		{
			if (AfterSaved != null)
			{
				AfterSaved(this, e);
			}
		}

		public EmploeeDialog(Activity context, Pharmacy pharmacy, /*ListView listView, */ Employee emploee = null)
		{
			if (context == null || pharmacy == null /*|| listView == null*/)
			{
				throw new ArgumentNullException();
			}

			this.context = context;
			this.pharmacy = pharmacy;
			this.emploee = emploee;
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

			if (emploee == null)
			{
				Dialog.SetTitle("НОВЫЙ СОТРУДНИК");
				emploee = MainDatabase.CreateEmploee(pharmacy.UUID);
			} 
			else 
			{
				Dialog.SetTitle("СОТРУДНИК : " + emploee.Name);
			}

			View view = inflater.Inflate(Resource.Layout.EmploeeDialog, container, false);

			view.FindViewById<EditText>(Resource.Id.edNameET).Text = emploee.Name;
			view.FindViewById<EditText>(Resource.Id.edPositionET).Text = emploee.Position;
			//if (emploee.BirthDate != null)
			//{
			//	view.FindViewById<DatePicker>(Resource.Id.edBirthDateDP).DateTime = emploee.BirthDate.Value.DateTime;
			//}
			view.FindViewById<EditText>(Resource.Id.edPhoneET).Text = emploee.Phone;
			view.FindViewById<EditText>(Resource.Id.edLoyaltyET).Text = emploee.Loyalty;
			view.FindViewById<CheckBox>(Resource.Id.edIsCustomerCB).Checked = emploee.IsCustomer;


			view.FindViewById<Button>(Resource.Id.edCloseB).Click += delegate {
				if (emploee.CreatedAt == null)
				{
					MainDatabase.DeleteEmploee(emploee);
				}

				transaction.Commit();

				Dismiss();
			};

			view.FindViewById<Button>(Resource.Id.edSaveB).Click += delegate {
				Toast.MakeText(context, "SAVE BUTTON CLICKED", ToastLength.Short).Show();
				emploee.CreatedAt = DateTimeOffset.Now;
				emploee.Name = view.FindViewById<EditText>(Resource.Id.edNameET).Text;
				emploee.Position = view.FindViewById<EditText>(Resource.Id.edPositionET).Text;
				//if (view.FindViewById<DatePicker>(Resource.Id.edBirthDateDP).DateTime != DateTime.MinValue)
				//{
				//	emploee.BirthDate = view.FindViewById<DatePicker>(Resource.Id.edBirthDateDP).DateTime;
				//}
				emploee.Phone = view.FindViewById<EditText>(Resource.Id.edPhoneET).Text;
				emploee.Loyalty = view.FindViewById<EditText>(Resource.Id.edLoyaltyET).Text;
				emploee.IsCustomer = view.FindViewById<CheckBox>(Resource.Id.edIsCustomerCB).Checked;

				//if (listView.Adapter is EmploeeAdapter)
				//{
				//	((EmploeeAdapter)listView.Adapter).NotifyDataSetChanged();
				//}

				transaction.Commit();

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
			this.emploee = null;
			//this.listView = null;
		}
	}
}

