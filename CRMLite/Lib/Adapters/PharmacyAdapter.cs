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

			view.FindViewById<TextView>(Resource.Id.ptiNameTV).Text = string.IsNullOrEmpty(item.Brand) ? @"<нет бренда>" : item.Brand;
			view.FindViewById<TextView>(Resource.Id.ptiAddressTV).Text = string.IsNullOrEmpty(item.Address) ? @"<нет адреса>" : item.Address;

			var showFinance = view.FindViewById<ImageView>(Resource.Id.ptiContractIV);
			showFinance.SetTag(Resource.String.PharmacyUUID, item.UUID);
			showFinance.Click -= ShowFinanceClickEventHandler;
			showFinance.Click += ShowFinanceClickEventHandler;

			var showHistory = view.FindViewById<ImageView>(Resource.Id.ptiHistoryIV);
			showHistory.SetTag(Resource.String.PharmacyUUID, item.UUID);
			showHistory.Click -= ShowHistoryClickEventHandler;
			showHistory.Click += ShowHistoryClickEventHandler;

			var showHospital = view.FindViewById<ImageView>(Resource.Id.ptiHospitalIV);
			showHospital.SetTag(Resource.String.PharmacyUUID, item.UUID);
			showHospital.Click -= ShowHospitalClickEventHandler;
			showHospital.Click += ShowHospitalClickEventHandler;

			var showEmployee = view.FindViewById<ImageView>(Resource.Id.ptiEmployeeIV);
			showEmployee.SetTag(Resource.String.PharmacyUUID, item.UUID);
			showEmployee.Click -= ShowEmployeeClickEventHandler;
			showEmployee.Click += ShowEmployeeClickEventHandler;


			var nexttAttendance = view.FindViewById<Button>(Resource.Id.ptiNextAttendanceB);
			nexttAttendance.SetTag(Resource.String.PharmacyUUID, item.UUID);
			nexttAttendance.Text = item.NextAttendanceDate.HasValue ? 
				item.NextAttendanceDate.Value.ToString(@"dd.MM.yyyy") : DateTimeOffset.Now.ToString(@"dd.MM.yyyy");
			nexttAttendance.Click -= NextAttendanceClickEventHandler;
			nexttAttendance.Click += NextAttendanceClickEventHandler;

			var lastAttendance = view.FindViewById<TextView>(Resource.Id.ptiLastAttendanceDateTV);
			lastAttendance.Text = item.LastAttendanceDate.HasValue ? 
				item.LastAttendanceDate.Value.ToString(@"dd.MM.yyyy") : @"<нет визита>";

			//Finally return the view
			return view;
		}

		void ShowFinanceClickEventHandler(object sender, EventArgs e)
		{
			if (sender is ImageView) {
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var financeAcivity = new Intent(context, typeof(FinanceActivity));
				financeAcivity.PutExtra(@"UUID", pharmacyUUID);
				context.StartActivity(financeAcivity);
			}
		}

		void ShowHistoryClickEventHandler(object sender, EventArgs e)
		{
			if (sender is ImageView) {
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var historyAcivity = new Intent(context, typeof(HistoryActivity));
				historyAcivity.PutExtra(@"UUID", pharmacyUUID);
				context.StartActivity(historyAcivity);
			}		
		}

		void ShowHospitalClickEventHandler(object sender, EventArgs e)
		{
			if (sender is ImageView)
			{
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var hospitalAcivity = new Intent(context, typeof(HospitalActivity));
				hospitalAcivity.PutExtra(HospitalActivity.C_PHARMACY_UUID, pharmacyUUID);
				context.StartActivity(hospitalAcivity);
			}
		}

		void ShowEmployeeClickEventHandler(object sender, EventArgs e)
		{
			if (sender is ImageView)
			{
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var employeeAcivity = new Intent(context, typeof(EmployeeActivity));
				employeeAcivity.PutExtra(EmployeeActivity.C_PHARMACY_UUID, pharmacyUUID);
				context.StartActivity(employeeAcivity);
			}
		}

		void NextAttendanceClickEventHandler(object sender, EventArgs e)
		{
			if (sender is Button)
			{
				var pharmacyUUID = ((Button)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var attendanceAcivity = new Intent(context, typeof(AttendanceActivity));
				attendanceAcivity.PutExtra(@"UUID", pharmacyUUID);
				context.StartActivity(attendanceAcivity);
			}
		}
	}
}
