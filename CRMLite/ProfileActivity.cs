using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite
{
	[Activity(Label = "ProfileActivity")]
	public class ProfileActivity : Activity
	{
		Pharmacy Pharmacy;
		LinearLayout Table;
		Dictionary<string, TextView> TextViews;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Profile);

			//FindViewById<Button>(Resource.Id.haCloseB).Click += (sender, e) => {
			//	Finish();
			//};

			Table = FindViewById<LinearLayout>(Resource.Id.paAttendanceByWeekTable);

			TextViews = new Dictionary<string, TextView>();
		}

		protected override void OnResume()
		{
			base.OnResume();
			RefreshView();
		}

		void RefreshView()
		{
			var watch = new Stopwatch();
			watch.Start();
			//var attendances = MainDatabase.GetEmployees(pharmacy.UUID).Where(> Datetime.Min).Sort();
			var attendances = MainDatabase.GetItems<Attendance>();
			var dict_key = string.Empty; // PharmacyUUID - Year - Week
			// 1. Рисуем таблицу
			int weeksCount = 14;
			DateTimeOffset[] dates = new DateTimeOffset[weeksCount];
			var header = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.AttendanceByWeekTableHeader, Table, false);
			for (int w = 0; w < weeksCount; w++) {
				dates[w] = DateTimeOffset.UtcNow.AddDays(-7 * (weeksCount - 1 - w));
				var hView = header.GetChildAt(w + 1);
				if (hView is TextView) {
					(hView as TextView).Text = Helper.GetIso8601WeekOfYear(dates[w].UtcDateTime).ToString();
				}
			}

			Table.AddView(header);

			foreach (var item in MainDatabase.GetItems<Pharmacy>()) {
				var row = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.AttendanceByWeekTableItem, Table, false);
				row.FindViewById<TextView>(Resource.Id.abwtiPharmacyTV).Text = item.GetName();
				//for (int v = 1; v <= datesCount; v++) {
				//	dict_key = string.Format("{0}-{1}-{2}", SKU.uuid, type, dates[v - 1].ToString(format));
				//	var view = row.GetChildAt(v);
				//	if (view is TextView) {
				//		(view as TextView).Text = string.Empty;
				//		TextViews.Add(dict_key, (view as TextView));
				//	}
				//}
				Table.AddView(row);
			}

			//var infoTypes = Enum.GetValues(typeof(DistributionInfoType)).Cast<DistributionInfoType>();
			//var drugSKUs = MainDatabase.GetDrugSKUs();

			//foreach (var type in infoTypes) {
			//	View subheader = LayoutInflater.Inflate(Resource.Layout.HistoryTableSubHeader, Table, false);
			//	subheader.FindViewById<TextView>(Resource.Id.htshTypeName).Text = GetTypeName(type);//type.ToString();
			//	Table.AddView(subheader);
			//	foreach (var SKU in drugSKUs) {
			//		var row = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.HistoryTableItem, Table, false);
			//		row.FindViewById<TextView>(Resource.Id.htiDrugSKUTV).Text = SKU.name;
			//		for (int v = 1; v <= datesCount; v++) {
			//			dict_key = string.Format("{0}-{1}-{2}", SKU.uuid, type, dates[v - 1].ToString(format));
			//			var view = row.GetChildAt(v);
			//			if (view is TextView) {
			//				(view as TextView).Text = string.Empty;
			//				TextViews.Add(dict_key, (view as TextView));
			//			}
			//		}
			//		Table.AddView(row);
			//	}
			//}

			//w.Stop();

			//Console.WriteLine(
			//	"{0}-{1}",
			//	w.ElapsedMilliseconds,
			//	TextViews.Count);

			//w.Restart();
			//// 2. Вставляем данные 
			//foreach (var attendance in attendances) {
			//	var distributions = MainDatabase.GetItems<DistributionData>().Where(i => i.Attendance == attendance.UUID);
			//	foreach (var distribution in distributions) {
			//		// IsExistence
			//		dict_key = string.Format("{0}-{1}-{2}", distribution.DrugSKU, DistributionInfoType.ditIsExistence, attendance.When.ToString(format));
			//		TextViews[dict_key].Text = distribution.IsExistence ? @"+" : @"-";

			//		// Count
			//		dict_key = string.Format("{0}-{1}-{2}", distribution.DrugSKU, DistributionInfoType.ditCount, attendance.When.ToString(format));
			//		TextViews[dict_key].Text = distribution.Count.ToString();

			//		// Price
			//		dict_key = string.Format("{0}-{1}-{2}", distribution.DrugSKU, DistributionInfoType.ditPrice, attendance.When.ToString(format));
			//		TextViews[dict_key].Text = distribution.Price.ToString();

			//		// IsPresence
			//		dict_key = string.Format("{0}-{1}-{2}", distribution.DrugSKU, DistributionInfoType.ditIsPresence, attendance.When.ToString(format));
			//		TextViews[dict_key].Text = distribution.IsPresence ? @"+" : @"-";

			//		// HasPOS
			//		dict_key = string.Format("{0}-{1}-{2}", distribution.DrugSKU, DistributionInfoType.ditHasPOS, attendance.When.ToString(format));
			//		TextViews[dict_key].Text = distribution.HasPOS ? @"+" : @"-";
			//	}
			//}

			watch.Stop();

			Console.WriteLine(
				"{0}-{1}",
				watch.ElapsedMilliseconds,
				TextViews.Count);
		}
	}
}

