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

		readonly string R_NoBrand;
		readonly string R_NoAddress;
		readonly string R_NoAttendance;

		public PharmacyAdapter(Activity context, IList<Pharmacy> pharmacies, string[] pharmaciesInRoute = null)
		{
			Context = context;
			Pharmacies = pharmacies;
			PharmaciesInRoute = pharmaciesInRoute;
			R_NoBrand = context.Resources.GetString(Resource.String.no_brand);
			R_NoAddress = context.Resources.GetString(Resource.String.no_address);
			R_NoAttendance = context.Resources.GetString(Resource.String.no_attendance);
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

		#region ViewHolder
		class ViewHolder : Java.Lang.Object
		{
			public TextView No { get; set; }
			public TextView State { get; set; }
			public TextView Name { get; set; }
			public TextView Address { get; set; }
			public TextView LastAttendance { get; set; }
			public Button NextAttendance { get; set; }
			public ImageView Finance { get; set; }
			public ImageView History { get; set; }
			public ImageView Hospital { get; set; }
			public ImageView Employee { get; set; }
		}
		#endregion

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = Pharmacies[position];
			var state = item.GetState();

			ViewHolder holder;
			var view = convertView;

			if (view == null) {
				view = Context.LayoutInflater.Inflate(Resource.Layout.PharmacyTableItem, parent, false);

				holder = new ViewHolder();
				holder.No = view.FindViewById<TextView>(Resource.Id.ptiNoTV);
				holder.State = view.FindViewById<TextView>(Resource.Id.ptiStateTV);
				holder.Name = view.FindViewById<TextView>(Resource.Id.ptiNameTV);
				holder.Address = view.FindViewById<TextView>(Resource.Id.ptiAddressTV);
				holder.LastAttendance = view.FindViewById<TextView>(Resource.Id.ptiLastAttendanceDateTV);

				holder.NextAttendance = view.FindViewById<Button>(Resource.Id.ptiNextAttendanceB);
				holder.NextAttendance.Click += NextAttendanceClickEventHandler;
				      
				holder.Finance = view.FindViewById<ImageView>(Resource.Id.ptiFinanceIV);
				holder.Finance.Click += ShowFinanceClickEventHandler;

				holder.History = view.FindViewById<ImageView>(Resource.Id.ptiHistoryIV);
				holder.History.Click += ShowHistoryClickEventHandler;

				holder.Hospital = view.FindViewById<ImageView>(Resource.Id.ptiHospitalIV);
				holder.Hospital.Click += ShowHospitalClickEventHandler;

				holder.Employee = view.FindViewById<ImageView>(Resource.Id.ptiEmployeeIV);
				holder.Employee.Click += ShowEmployeeClickEventHandler;

				view.Tag = holder;
			} else {
				holder = view.Tag as ViewHolder;
			}

			holder.No.Text = (position + 1).ToString();
			holder.State.Text = GetStateDesc(state);
			holder.Name.Text = string.IsNullOrEmpty(item.Brand) ? R_NoBrand : item.Brand;
			holder.Address.Text = string.IsNullOrEmpty(item.Address) ? R_NoAddress : item.Address;

			holder.Finance.SetTag(Resource.String.PharmacyUUID, item.UUID);

			holder.History.SetTag(Resource.String.PharmacyUUID, item.UUID);

			holder.Hospital.SetTag(Resource.String.PharmacyUUID, item.UUID);

			holder.Employee.SetTag(Resource.String.PharmacyUUID, item.UUID);


			holder.NextAttendance.SetTag(Resource.String.PharmacyUUID, item.UUID);
			holder.NextAttendance.Text = item.NextAttendanceDate.HasValue ? 
				item.NextAttendanceDate.Value.ToString(@"dd.MM.yyyy") : @"Начать визит";

			holder.LastAttendance.Text = item.LastAttendanceDate.HasValue ? 
				item.LastAttendanceDate.Value.ToString(@"dd.MM.yyyy") : R_NoAttendance;

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
								holder.NextAttendance.Text = @"Пройдено!";
							} else {
								holder.NextAttendance.Text = @"Начать визит";
							}
						} else {
							holder.NextAttendance.Text = @"Начать визит";
						}
					} else {
						switch (state) {
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
					switch (state) {
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
