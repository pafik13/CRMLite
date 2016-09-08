
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
	public enum DistributionInfoType { ditIsExistence, ditCount, ditPrice, ditIsPresence, ditHasPOS, ditOrder };

	[Activity(Label = "HistoryActivity")]
	public class HistoryActivity : Activity
	{
		Pharmacy Pharmacy;
		LinearLayout Table;
		Dictionary<string, TextView> TextViews;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);

			// Create your application here

			SetContentView(Resource.Layout.History);

			FindViewById<Button>(Resource.Id.haCloseB).Click += (sender, e) => {
				Finish();
			};

			var pharmacyUUID = Intent.GetStringExtra("UUID");
			if (string.IsNullOrEmpty(pharmacyUUID)) {
				return;
			} else {

				Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
				FindViewById<TextView>(Resource.Id.haInfoTV).Text = string.Format("ИСТОРИЯ ВИЗИТОВ: {0}", Pharmacy.GetName());

//				var date1 = FindViewById<TextView>(Resource.Id.htiDate1);
//				date1.Text = DateTimeOffset.Now.Date.ToString("dd.MM.yy");
//
//				var date2 = FindViewById<TextView>(Resource.Id.htiDate2);
//				date2.Text = DateTimeOffset.Now.Date.AddDays(7).Date.ToString("dd.MM.yy");
//
				Table = FindViewById<LinearLayout>(Resource.Id.haTable);
//				for (int i = 0; i < 60; i++) {
//					var view = LayoutInflater.Inflate(Resource.Layout.HistoryTableItem, table, true);
//				}
			}
			TextViews = new Dictionary<string, TextView>();
		}

		protected override void OnResume()
		{
			base.OnResume();

			if (Pharmacy == null) {
				new AlertDialog.Builder(this)
							   .SetTitle(Resource.String.error_caption)
							   .SetMessage("Отсутствует аптека!")
							   .SetCancelable(false)
							   .SetPositiveButton(@"OK", (dialog, args) => {
								   if (dialog is Dialog) {
									   ((Dialog)dialog).Dismiss();
									   Finish();
								   }
							   })
							   .Show();
			} else {
				RefreshView();
			}
		}

		void RefreshView()
		{
			Stopwatch s1 = new Stopwatch();
			s1.Start();
			//var attendances = MainDatabase.GetEmployees(pharmacy.UUID).Where(> Datetime.Min).Sort();
			var attendances = MainDatabase.GetItems<Attendance>().Where(i => i.Pharmacy == Pharmacy.UUID).OrderBy(i => i.When).ToList();
			var dict_key = string.Empty;
			// 1. Рисуем таблицу
			int dateStep = 7;
			int datesCount = 20;
			string format = @"ddMMyy";
			DateTimeOffset[] dates = new DateTimeOffset[datesCount];
			var header = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.HistoryTableHeader, Table, false);
			for (int d = 0; d < datesCount; d++) {
				if (d < attendances.Count) {
					dates[d] = attendances[d].When;
				} else {
					dates[d] = d == 0 ? DateTimeOffset.Now : dates[d - 1].AddDays(dateStep);
				}
				var hView = header.GetChildAt(d + 1);
				if (hView is TextView) {
					(hView as TextView).Text = dates[d].ToString(
						string.Format("dd{0}MMM{1}yyyy",System.Environment.NewLine,System.Environment.NewLine)
					);
				}
			}

			Table.AddView(header);

			var infoTypes = Enum.GetValues(typeof(DistributionInfoType)).Cast<DistributionInfoType>();
			var drugSKUs = MainDatabase.GetDrugSKUs();

			foreach (var type in infoTypes) {
				View subheader = LayoutInflater.Inflate(Resource.Layout.HistoryTableSubHeader, Table, false);
				subheader.FindViewById<TextView>(Resource.Id.htshTypeName).Text = GetTypeName(type);//type.ToString();
				Table.AddView(subheader);
				foreach (var SKU in drugSKUs) {
					var row = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.HistoryTableItem, Table, false);
					row.FindViewById<TextView>(Resource.Id.htiDrugSKUTV).Text = SKU.name;
					for (int v = 1; v <= datesCount; v++) {
						dict_key = string.Format("{0}-{1}-{2}", SKU.uuid, type, dates[v - 1].ToString(format));
						var view = row.GetChildAt(v);
						if (view is TextView) {
							(view as TextView).Text = string.Empty;
							TextViews.Add(dict_key, (view as TextView));
						}
					}
					Table.AddView(row);
				}
			}

			// 2. Вставляем данные 
			foreach (var attendance in attendances) {
				var distributions = MainDatabase.GetItems<DistributionData>().Where(i => i.Attendance == attendance.UUID);
				foreach (var distribution in distributions) {
					// IsExistence
					dict_key = string.Format("{0}-{1}-{2}", distribution.DrugSKU, DistributionInfoType.ditIsExistence, attendance.When.ToString(format));
					TextViews[dict_key].Text = distribution.IsExistence ? @"+" : @"-";

					// Count
					dict_key = string.Format("{0}-{1}-{2}", distribution.DrugSKU, DistributionInfoType.ditCount, attendance.When.ToString(format));
					TextViews[dict_key].Text = distribution.Count.ToString();

					// Price
					dict_key = string.Format("{0}-{1}-{2}", distribution.DrugSKU, DistributionInfoType.ditPrice, attendance.When.ToString(format));
					TextViews[dict_key].Text = distribution.Price.ToString();

					// IsPresence
					dict_key = string.Format("{0}-{1}-{2}", distribution.DrugSKU, DistributionInfoType.ditIsPresence, attendance.When.ToString(format));
					TextViews[dict_key].Text = distribution.IsPresence ? @"+" : @"-";

					// HasPOS
					dict_key = string.Format("{0}-{1}-{2}", distribution.DrugSKU, DistributionInfoType.ditHasPOS, attendance.When.ToString(format));
					TextViews[dict_key].Text = distribution.HasPOS ? @"+" : @"-";
				}
			}

			s1.Stop();

			Console.WriteLine(
				"{0}-{1}",
				s1.ElapsedMilliseconds,
				TextViews.Count);
		}

		string GetTypeName(DistributionInfoType infoType)
		{
			switch (infoType) {
				case DistributionInfoType.ditIsExistence:
					return @"Наличие";
				case DistributionInfoType.ditCount:
					return @"Количество";
				case DistributionInfoType.ditPrice:
					return @"Цена";
				case DistributionInfoType.ditIsPresence:
					return @"Выкладка";
				case DistributionInfoType.ditHasPOS:
					return @"POS-материалы";
				default:
					return @"Unknown";
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
		}

		protected override void OnStop()
		{
			base.OnStop();
		}
	}
}

