using System;
using System.Linq;
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
		readonly Activity Context;
		readonly IList<Pharmacy> Pharmacies;
		readonly string[] PharmaciesInRoute;

		public PharmacyAdapter(Activity context, IList<Pharmacy> pharmacies, string[] pharmaciesInRoute = null) : base()
		{
			Context = context;
			Pharmacies = pharmacies;
			PharmaciesInRoute = pharmaciesInRoute;
		}

		public override Pharmacy this[int position]
		{
			get { 
				return Pharmacies[position]; 
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { 
				return Pharmacies.Count; 
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = Pharmacies[position];

			var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.PharmacyTableItem, parent, false)
			           ) as LinearLayout;
    		view.FindViewById<TextView>(Resource.Id.ptiStateTV).Text = GetStateDesc(item.GetState());
			view.FindViewById<TextView>(Resource.Id.ptiNameTV).Text = 
				string.IsNullOrEmpty(item.Brand) ? @"<нет бренда>" : item.Brand;
			view.FindViewById<TextView>(Resource.Id.ptiAddressTV).Text = 
				string.IsNullOrEmpty(item.Address) ? @"<нет адреса>" : item.Address;

			var showFinance = view.FindViewById<ImageView>(Resource.Id.ptiFinanceIV);
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


			var nextAttendance = view.FindViewById<Button>(Resource.Id.ptiNextAttendanceB);
			nextAttendance.SetTag(Resource.String.PharmacyUUID, item.UUID);
			nextAttendance.Text = item.NextAttendanceDate.HasValue ? 
				item.NextAttendanceDate.Value.ToString(@"dd.MM.yyyy") : @"Начать визит"; //DateTimeOffset.Now.ToString(@"dd.MM.yyyy");
			nextAttendance.Click -= NextAttendanceClickEventHandler;
			nextAttendance.Click += NextAttendanceClickEventHandler;

			var lastAttendance = view.FindViewById<TextView>(Resource.Id.ptiLastAttendanceDateTV);
			lastAttendance.Text = item.LastAttendanceDate.HasValue ? 
				item.LastAttendanceDate.Value.ToString(@"dd.MM.yyyy") : @"<нет визита>";

			if (PharmaciesInRoute == null) return view;
			if (PharmaciesInRoute.Length == 0) return view;

			// wmOnlyRoute, wmRouteAndRecommendations, wmOnlyRecommendations
			switch (Helper.WorkMode) {
				case WorkMode.wmOnlyRoute:
				case WorkMode.wmRouteAndRecommendations:
					if (PharmaciesInRoute.Contains(item.UUID)) {
						view.SetBackgroundResource(Resource.Color.Light_Green_100);
						if (item.LastAttendanceDate.HasValue) {
							if (item.LastAttendanceDate.Value.UtcDateTime.Date == DateTimeOffset.UtcNow.Date) {
								nextAttendance.Text = @"Пройдено!";
							} else {
								nextAttendance.Text = @"Начать визит";
							}
						} else {
							nextAttendance.Text = @"Начать визит";
						}
					} else {
						switch (item.GetState()) {
							case PharmacyState.psActive:
								view.SetBackgroundColor(Android.Graphics.Color.White);
								break;
							case PharmacyState.psReserve:
								view.SetBackgroundResource(Resource.Color.Yellow_100);
								break;
							case PharmacyState.psClose:
								view.SetBackgroundResource(Resource.Color.Red_100);
								break;
						}
					}
					break;
				case WorkMode.wmOnlyRecommendations:
					switch (item.GetState()) {
						case PharmacyState.psActive:
							view.SetBackgroundColor(Android.Graphics.Color.White);
							break;
						case PharmacyState.psReserve:
							view.SetBackgroundResource(Resource.Color.Yellow_100);
							break;
						case PharmacyState.psClose:
							view.SetBackgroundResource(Resource.Color.Red_100);
							break;
					}
					break;
			}

			//Finally return the view
			return view;
		}
		
		string GetStateDesc(PharmacyState state)
		{
			switch(state){
				case PharmacyState.psActive:
					return "Активная";
				case PharmacyState.psReserve:
					return "Резервная";
				case PharmacyState.psClose:
					return "Закрытая";
				default:
					return @"<Unknown>";
		  }
		}

		void ShowFinanceClickEventHandler(object sender, EventArgs e)
		{
			if (sender is ImageView) {
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var financeAcivity = new Intent(Context, typeof(FinanceActivity));
				financeAcivity.PutExtra(@"UUID", pharmacyUUID);
				Context.StartActivity(financeAcivity);
			}
		}

		void ShowHistoryClickEventHandler(object sender, EventArgs e)
		{
			if (sender is ImageView) {
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var historyAcivity = new Intent(Context, typeof(HistoryActivity));
				historyAcivity.PutExtra(@"UUID", pharmacyUUID);
				Context.StartActivity(historyAcivity);
			}		
		}

		void ShowHospitalClickEventHandler(object sender, EventArgs e)
		{
			if (sender is ImageView)
			{
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var hospitalAcivity = new Intent(Context, typeof(HospitalActivity));
				hospitalAcivity.PutExtra(HospitalActivity.C_PHARMACY_UUID, pharmacyUUID);
				Context.StartActivity(hospitalAcivity);
			}
		}

		void ShowEmployeeClickEventHandler(object sender, EventArgs e)
		{
			if (sender is ImageView)
			{
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var employeeAcivity = new Intent(Context, typeof(EmployeeActivity));
				employeeAcivity.PutExtra(EmployeeActivity.C_PHARMACY_UUID, pharmacyUUID);
				Context.StartActivity(employeeAcivity);
			}
		}

		void NextAttendanceClickEventHandler(object sender, EventArgs e)
		{
			if (sender is Button)
			{
				var pharmacyUUID = ((Button)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var attendanceAcivity = new Intent(Context, typeof(AttendanceActivity));
				attendanceAcivity.PutExtra(@"UUID", pharmacyUUID);
				Context.StartActivity(attendanceAcivity);
			}
		}
	}
}
