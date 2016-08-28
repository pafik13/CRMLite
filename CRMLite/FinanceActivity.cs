
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

using CRMLite.Dialogs;
using CRMLite.Entities;

namespace CRMLite
{
	public enum FinanceInfoType { fitSale, fitPurchase, fitRemain };

	[Activity(Label = "FinanceActivity")]
	public class FinanceActivity : Activity
	{
		const string PeriodFormatForKey = @"MMyy";

		Pharmacy Pharmacy;
		LinearLayout Table;
		Dictionary<string, TextView> TextViews;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);

			// Create your application here

			SetContentView(Resource.Layout.Finance);

			FindViewById<Button>(Resource.Id.faCloseB).Click += (sender, e) => {
				Finish();
			};

			var pharmacyUUID = Intent.GetStringExtra("UUID");
			if (string.IsNullOrEmpty(pharmacyUUID)) return;


			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
			FindViewById<TextView>(Resource.Id.faInfoTV).Text = string.Format("ПРОДАЖИ: {0}", Pharmacy.GetName());

			//				var date1 = FindViewById<TextView>(Resource.Id.htiDate1);
			//				date1.Text = DateTimeOffset.Now.Date.ToString("dd.MM.yy");
			//
			//				var date2 = FindViewById<TextView>(Resource.Id.htiDate2);
			//				date2.Text = DateTimeOffset.Now.Date.AddDays(7).Date.ToString("dd.MM.yy");
			//
			Table = FindViewById<LinearLayout>(Resource.Id.faTable);


			var add = FindViewById<ImageView>(Resource.Id.faAdd);
			add.Click += (sender, e) => {
				//Toast.MakeText(this, "ADD BUTTON CLICKED", ToastLength.Short).Show();
				Console.WriteLine("Event {0} was called", "faAdd_Click");
				FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();
				FinanceDialog financeDialog = new FinanceDialog(Pharmacy);
				financeDialog.Show(fragmentTransaction, "FinanceDialog");
				financeDialog.AfterSaved += (caller, arguments) => {
					Console.WriteLine("Event {0} was called. FinanceDatas count {1}", "AfterSaved", arguments.FinanceDatas.Count);

					//Table.RemoveAllViews();

					//RefreshView();
					SetValues(arguments.FinanceDatas);
				};
			};
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
			TextViews = new Dictionary<string, TextView>();

			Stopwatch s1 = new Stopwatch();
			s1.Start();

			var financeDatas = MainDatabase.GetItems<FinanceData>().Where(i => i.Pharmacy == Pharmacy.UUID).ToList();
			var dict_key = string.Empty;

			// 1. Рисуем таблицу
			int monthesCount = 10;
			string format = @"MMyy";
			DateTimeOffset[] financeDatasDates = financeDatas.Select(i => i.Period).Distinct().OrderBy(i => i).ToArray();
			DateTimeOffset[] dates = new DateTimeOffset[monthesCount];
			var header = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.FinanceTableHeader, Table, false);
			for (int m = 0; m < monthesCount; m++) {
				if (m < financeDatasDates.Length) {
					dates[m] = financeDatasDates[m];
				} else {
					dates[m] = dates[m - 1].AddMonths(1);
				}
				var hView = header.GetChildAt(m + 1);
				if (hView is TextView) {
					(hView as TextView).Text = dates[m].ToString(string.Format(@"MMMM{0}yyyy", System.Environment.NewLine));
				}
			}

			Table.AddView(header);

			var infoTypes = Enum.GetValues(typeof(FinanceInfoType)).Cast<FinanceInfoType>();
			var drugSKUs = MainDatabase.GetDrugSKUs();

			foreach (var type in infoTypes) {
				View subheader = LayoutInflater.Inflate(Resource.Layout.FinanceTableSubHeader, Table, false);
				subheader.FindViewById<TextView>(Resource.Id.ftshTypeName).Text = GetTypeName(type);//type.ToString();
				Table.AddView(subheader);
				foreach (var SKU in drugSKUs) {
					var row = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.FinanceTableItem, Table, false);
					row.FindViewById<TextView>(Resource.Id.ftiDrugSKUTV).Text = SKU.name;
					for (int v = 1; v <= monthesCount; v++) {
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
			foreach (var financeData in financeDatas) {
				// Salee
				dict_key = string.Format("{0}-{1}-{2}", financeData.DrugSKU, FinanceInfoType.fitSale, financeData.Period.ToString(format));
				TextViews[dict_key].Text = financeData.Sale.ToString();

				// Purchase
				dict_key = string.Format("{0}-{1}-{2}", financeData.DrugSKU, FinanceInfoType.fitPurchase, financeData.Period.ToString(format));
				TextViews[dict_key].Text = financeData.Purchase.ToString();

				// Remain 
				dict_key = string.Format("{0}-{1}-{2}", financeData.DrugSKU, FinanceInfoType.fitRemain, financeData.Period.ToString(format));
				TextViews[dict_key].Text = financeData.Remain.ToString();
			}


			s1.Stop();

			Console.WriteLine(
				"{0}-{1}",
				s1.ElapsedMilliseconds,
				TextViews.Count);
		}

		void SetValues(List<FinanceData> financeDatas)
		{
			var key = string.Empty;
			// 2. Вставляем данные 
			foreach (var financeData in financeDatas) {
				// Salee
				key = string.Format("{0}-{1}-{2}", financeData.DrugSKU, FinanceInfoType.fitSale, financeData.Period.ToString(PeriodFormatForKey));
				TextViews[key].Text = financeData.Sale.ToString();

				// Purchase
				key = string.Format("{0}-{1}-{2}", financeData.DrugSKU, FinanceInfoType.fitPurchase, financeData.Period.ToString(PeriodFormatForKey));
				TextViews[key].Text = financeData.Purchase.ToString();

				// Remain 
				key = string.Format("{0}-{1}-{2}", financeData.DrugSKU, FinanceInfoType.fitRemain, financeData.Period.ToString(PeriodFormatForKey));
				TextViews[key].Text = financeData.Remain.ToString();
			}
		}

		string GetTypeName(FinanceInfoType infoType)
		{
			switch (infoType) {
				case FinanceInfoType.fitSale:
					return @"Продажи";
				case FinanceInfoType.fitPurchase:
					return @"Закуп";
				case FinanceInfoType.fitRemain:
					return @"Остаток";
				default:
					return @"Unknown";
			}
		}

		void AddFinanceData(string pharmacyUUID, string skuUUID, DateTimeOffset period, float? sale, float? purchase, float? remain)
		{
			var financeData = MainDatabase.Create<FinanceData>();
			financeData.Pharmacy = pharmacyUUID;
			financeData.Period = period;
			financeData.DrugSKU = skuUUID;
			financeData.Sale = sale;
			financeData.Purchase = purchase;
			financeData.Remain = remain;
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

