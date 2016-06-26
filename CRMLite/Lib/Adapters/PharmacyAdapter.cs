using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class PharmacyAdapter : BaseAdapter<Pharmacy>
	{
		Activity context = null;
		IList<Pharmacy> pharmacies = new List<Pharmacy>();

		public PharmacyAdapter(Activity context, IList<Pharmacy> pharmacies) : base()
		{
			this.context = context;
			this.pharmacies = pharmacies;
		}

		public override Pharmacy this[int position]
		{
			get { return pharmacies[position]; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { return pharmacies.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = pharmacies[position];

			var view = (convertView ??
								context.LayoutInflater.Inflate(
				            	Resource.Layout.PharmacyTableItem,
								parent,
								false)) as LinearLayout;
			
			view.FindViewById<TextView>(Resource.Id.ptiNameTV).Text = string.IsNullOrEmpty(item.LegalName) ? "<unknow name>" : item.LegalName;
			view.FindViewById<TextView>(Resource.Id.ptiAddressTV).Text = string.IsNullOrEmpty(item.Address) ? "<unknow address>" : item.Address;

			//view.FindViewById<Button>(Resource.Id.ptiLastAttendanceDateB).Click += delegate {
			//	Toast.MakeText(context, "Нажали на кнопку!", ToastLength.Short).Show();
			//};

			var showHospital = view.FindViewById<ImageView>(Resource.Id.ptiHospitalIV);
			showHospital.SetTag(Resource.String.PharmacyUUID, item.UUID);
			showHospital.Click -= ShowHospitalClickEventHandler;
			showHospital.Click += ShowHospitalClickEventHandler;

			var showEmployee = view.FindViewById<ImageView>(Resource.Id.ptiEmployeeIV);
			showEmployee.SetTag(Resource.String.PharmacyUUID, item.UUID);
			showEmployee.Click -= ShowEmployeeClickEventHandler;
			showEmployee.Click += ShowEmployeeClickEventHandler;

			//Finally return the view
			return view;
		}

		void ShowHospitalClickEventHandler(object sender, EventArgs e)
		{
			if (sender is ImageView)
			{
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var hospitalAcivity = new Intent(context, typeof(HospitalActivity));
				hospitalAcivity.PutExtra(@"UUID", pharmacyUUID);
				context.StartActivity(hospitalAcivity);
			}
		}

		void ShowEmployeeClickEventHandler(object sender, System.EventArgs e)
		{
			if (sender is ImageView)
			{
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var employeeAcivity = new Intent(context, typeof(EmployeeActivity));
				employeeAcivity.PutExtra(@"UUID", pharmacyUUID);
				context.StartActivity(employeeAcivity);
			}
		}
	}
}
