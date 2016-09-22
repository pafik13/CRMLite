﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
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
		const string PeriodFormatForKey = @"MMyyyy";
		public const string C_IS_CAN_ADD = @"C_IS_CAN_ADD";
		
		Pharmacy Pharmacy;
		LinearLayout Content;
		LinearLayout Table;
		Dictionary<string, TextView> TextViews;

		LinearLayout TableBySKU;
		Dictionary<string, TextView> TextViewsBySKU;

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
			Content = FindViewById<LinearLayout>(Resource.Id.faContentLL);
			Table = FindViewById<LinearLayout>(Resource.Id.faTable);
			TableBySKU = FindViewById<LinearLayout>(Resource.Id.faTableBySKU);

			var add = FindViewById<ImageView>(Resource.Id.faAdd);
			var isCanAdd = Intent.GetBooleanExtra(C_IS_CAN_ADD, true);
			if (isCanAdd){
				add.Click += (sender, e) => {
					//Toast.MakeText(this, "ADD BUTTON CLICKED", ToastLength.Short).Show();
					Console.WriteLine("Event {0} was called", "faAdd_Click");
					FragmentTransaction fragmentTransaction = FragmentManager.BeginTransaction();

					var prev = FragmentManager.FindFragmentByTag(FinanceDialog.TAG);
					if (prev != null) {
						fragmentTransaction.Remove(prev);
					}
					fragmentTransaction.AddToBackStack(null);

					var financeDialog = new FinanceDialog(Pharmacy);
					financeDialog.Show(fragmentTransaction, FinanceDialog.TAG);
					financeDialog.AfterSaved += (caller, arguments) => {
						Console.WriteLine("Event {0} was called. FinanceDatas count {1}", "AfterSaved", arguments.FinanceDatas.Count);

						//Table.RemoveAllViews();

						RefreshView();
						//MainDatabase.SaveItems(arguments.FinanceDatas);
						//SetValues(arguments.FinanceDatas);
					};
				};
			} else {
				add.Visibility = ViewStates.Gone;
			}
			
			var switcher = FindViewById<ViewSwitcher>(Resource.Id.faSwitchViewTypeVS);
			var sw = FindViewById<ImageView>(Resource.Id.faSwitchIV);
			sw.Click += (sender, e) => {
				switcher.ShowNext();
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
				//RefreshViewBySKU();
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

		void RefreshView()
		{
			Table.RemoveAllViews();

			TextViews = new Dictionary<string, TextView>();

			// TODO: remove Stopwatchers
			var generateViews = new Stopwatch();
			generateViews.Start();

			//var financeDatas = MainDatabase.GetItems<FinanceData>().Where(i => i.Pharmacy == Pharmacy.UUID).ToList();
			var dict_key = string.Empty;

			// 1. Рисуем таблицу
			int monthesCount = 9;
			DateTimeOffset[] months = new DateTimeOffset[monthesCount];
			int quartersCount = monthesCount / 3;
			int[] quarters = new int[quartersCount];

			DateTimeOffset baseDate = DateTimeOffset.Now.AddMonths(2);

			int highMonth = ((baseDate.Month + 2) / 3 * 3);
			DateTimeOffset highDate = baseDate.AddMonths(highMonth - baseDate.Month);
			for (int m = 0, q = 1; m < monthesCount; m++, q++) {
				months[m] = highDate.AddMonths(m - monthesCount + 1);

				if ((q % 3) == 0) {
					quarters[(q / 3) - 1] = (months[m].Month + 2) / 3;
				}
			}

			var header = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.FinanceTableHeader, Table, false);
			for (int col = 0; col < (monthesCount + quartersCount); col++) {
				
				var hView = header.GetChildAt(col + 1);
				if (hView is TextView) {
					if ((col + 1) % 4 == 0) {
						(hView as TextView).Text = months[col - (col / 4) - 1].ToString(string.Format(@"{0} кв.{1}yyyy", quarters[col / 4], System.Environment.NewLine));
					} else {
						(hView as TextView).Text = months[col - (col / 4)].ToString(string.Format(@"MMMM{0}yyyy", System.Environment.NewLine));
					}
				}
			}

			Content.AddView(header, 0);
			//Table.AddView(header);

			var infoTypes = Enum.GetValues(typeof(FinanceInfoType)).Cast<FinanceInfoType>();
			var drugSKUs = MainDatabase.GetDrugSKUs();

			foreach (var type in infoTypes) {
				View subheader = LayoutInflater.Inflate(Resource.Layout.FinanceTableSubHeader, Table, false);
				subheader.FindViewById<TextView>(Resource.Id.ftshTypeName).Text = GetTypeName(type);//type.ToString();
				Table.AddView(subheader);
				foreach (var SKU in drugSKUs) {
					var row = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.FinanceTableItem, Table, false);
					row.FindViewById<TextView>(Resource.Id.ftiDrugSKUTV).Text = SKU.name;
					for (int col = 0; col < (monthesCount + quartersCount); col++) {
						var view = row.GetChildAt(col + 1);
						if (view is TextView) {
							(view as TextView).Text = string.Empty;

							if ((col + 1) % 4 == 0) {
								string datePart = months[col - (col / 4) - 1].ToString(string.Format(@"4{0}yyyy", quarters[col / 4]));
								dict_key = string.Format("{0}-{1}-{2}", SKU.uuid, type, datePart);
							} else {
								string datePart = months[col - (col / 4)].ToString(PeriodFormatForKey);
								dict_key = string.Format("{0}-{1}-{2}", SKU.uuid, type, datePart);
							}

							TextViews.Add(dict_key, (view as TextView));
						}
					}

					Table.AddView(row);
				}
			}

			generateViews.Stop();

			Console.WriteLine(
				"FinanceData: RefreshView={0}-{1}",
				generateViews.ElapsedMilliseconds,
				TextViews.Count);

			var setValues = new Stopwatch();
			setValues.Start();

			var key = string.Empty;
			// 2. Данные
			TextView txt;

			// 2.1 Данные с визита
			// 2.1.1 Вставляем данные за месяц
			var sdMonths = MainDatabase.GetPharmacyDatas<SaleDataByMonth>(Pharmacy.UUID);
			foreach (var data in sdMonths) {
				string datePart = string.Format("{0}{1}", data.Month.ToString("D2"), data.Year);
				// Sale
				key = string.Format("{0}-{1}-{2}", data.DrugSKU, FinanceInfoType.fitSale, datePart);
				if (TextViews.ContainsKey(key)) {
					txt = TextViews[key];
					txt.Text = data.Sale.ToString();
					txt.SetTextColor(Android.Graphics.Color.DarkOrange);
				}
			}
			// 2.1.2 Вставляем данные за квартал
			var sdQuarters = MainDatabase.GetPharmacyDatas<SaleDataByQuarter>(Pharmacy.UUID);
			foreach (var data in sdQuarters) {
				string datePart = string.Format("4{0}{1}", data.Quarter, data.Year);
				// Sale
				key = string.Format("{0}-{1}-{2}", data.DrugSKU, FinanceInfoType.fitSale, datePart);
				if (TextViews.ContainsKey(key)) {
					txt = TextViews[key];
					txt.Text = data.Sale.ToString();
					txt.SetTextColor(Android.Graphics.Color.DarkOrange);
				}
			}

			// 2.2 Данные от сетей введенные
			// 2.2.1 Вставляем данные за месяц 
			var fdMonths = MainDatabase.GetPharmacyDatas<FinanceDataByMonth>(Pharmacy.UUID);
			foreach (var data in fdMonths) {
				string datePart = string.Format("{0}{1}", data.Month.ToString("D2"), data.Year);
				// Sale
				key = string.Format("{0}-{1}-{2}", data.DrugSKU, FinanceInfoType.fitSale, datePart);
				if (TextViews.ContainsKey(key)) {
					txt = TextViews[key];
					txt.Text = data.Sale.ToString();
					txt.SetTextColor(Android.Graphics.Color.Blue);
				}
				//TextViewsBySKU[key].Text = financeData.Sale.ToString();
				// Purchase
				key = string.Format("{0}-{1}-{2}", data.DrugSKU, FinanceInfoType.fitPurchase, datePart);
				if (TextViews.ContainsKey(key)) {
					txt = TextViews[key];
					txt.Text = data.Purchase.ToString();
					txt.SetTextColor(Android.Graphics.Color.Blue);
				}
				//TextViewsBySKU[key].Text = financeData.Purchase.ToString();

				// Remain 
				key = string.Format("{0}-{1}-{2}", data.DrugSKU, FinanceInfoType.fitRemain, datePart);
				if (TextViews.ContainsKey(key)) {
					txt = TextViews[key];
					txt.Text = data.Remain.ToString();
					txt.SetTextColor(Android.Graphics.Color.Blue);
				}
				//TextViewsBySKU[key].Text = financeData.Remain.ToString();
			}

			// 2.2.2 Вставляем данные за квартал
			var fdQuarters = MainDatabase.GetPharmacyDatas<FinanceDataByQuarter>(Pharmacy.UUID);
			foreach (var data in fdQuarters) {
				string datePart = string.Format("4{0}{1}", data.Quarter, data.Year);
				// Sale
				key = string.Format("{0}-{1}-{2}", data.DrugSKU, FinanceInfoType.fitSale, datePart);
				if (TextViews.ContainsKey(key)) {
					txt = TextViews[key];
					txt.Text = data.Sale.ToString();
					txt.SetTextColor(Android.Graphics.Color.Blue);
				}
				//TextViewsBySKU[key].Text = financeData.Sale.ToString();
				// Purchas
				key = string.Format("{0}-{1}-{2}", data.DrugSKU, FinanceInfoType.fitPurchase, datePart);
				if (TextViews.ContainsKey(key)) {
					txt = TextViews[key];
					txt.Text = data.Purchase.ToString();
					txt.SetTextColor(Android.Graphics.Color.Blue);				}
				//TextViewsBySKU[key].Text = financeData.Purchase.ToString();

				// Remain
				key = string.Format("{0}-{1}-{2}", data.DrugSKU, FinanceInfoType.fitRemain, datePart);
				if (TextViews.ContainsKey(key)) {
					txt = TextViews[key];
					txt.Text = data.Remain.ToString();
					txt.SetTextColor(Android.Graphics.Color.Blue);				}
				//TextViewsBySKU[key].Text = financeData.Remain.ToString();
			}

			setValues.Stop();
			Console.WriteLine(
				"FinanceData: SetValues={0}-{1}-{2}-{3}-{4}",
				setValues.ElapsedMilliseconds,
				sdMonths.Count, sdQuarters.Count, fdMonths.Count, fdQuarters.Count);

			//RefreshViewBySKU(financeDatas, dates, infoTypes, drugSKUs);
		}

		void SetValues(List<FinanceData> financeDatas)
		{
			var setValues = new Stopwatch();
			setValues.Start();

			var key = string.Empty;
			// 2. Вставляем данные 
			foreach (var financeData in financeDatas) {
				// Sale
				key = string.Format("{0}-{1}-{2}", financeData.DrugSKU, FinanceInfoType.fitSale, financeData.Period.ToString(PeriodFormatForKey));
				TextViews[key].Text = financeData.Sale.ToString();
				TextViewsBySKU[key].Text = financeData.Sale.ToString();
				// Purchase
				key = string.Format("{0}-{1}-{2}", financeData.DrugSKU, FinanceInfoType.fitPurchase, financeData.Period.ToString(PeriodFormatForKey));
				TextViews[key].Text = financeData.Purchase.ToString();
				TextViewsBySKU[key].Text = financeData.Purchase.ToString();

				// Remain 
				key = string.Format("{0}-{1}-{2}", financeData.DrugSKU, FinanceInfoType.fitRemain, financeData.Period.ToString(PeriodFormatForKey));
				TextViews[key].Text = financeData.Remain.ToString();
				TextViewsBySKU[key].Text = financeData.Remain.ToString();
			}

			setValues.Stop();

			Console.WriteLine(
				"FinanceData: SetValues={0}-{1}",
				setValues.ElapsedMilliseconds,
				TextViewsBySKU.Count);
		}


		void RefreshViewBySKU(List<FinanceData> financeDatas, DateTimeOffset[] dates, IEnumerable<FinanceInfoType> infoTypes, IList<DrugSKU> drugSKUs)
		{
			TextViewsBySKU = new Dictionary<string, TextView>();
			//var TableBySKU = new LinearLayout(this);

			var generateViews = new Stopwatch();
			generateViews.Start();

			var dict_key = string.Empty;

			// 1. Рисуем таблицу
			var header = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.FinanceTableHeader, TableBySKU, false);
			for (int m = 0; m < dates.Count(); m++) {
				var hView = header.GetChildAt(m + 1);
				if (hView is TextView) {
					(hView as TextView).Text = dates[m].ToString(string.Format(@"MMMM{0}yyyy", System.Environment.NewLine));
				}
			}

			TableBySKU.AddView(header);

			foreach (var SKU in drugSKUs) {	
				View subheader = LayoutInflater.Inflate(Resource.Layout.FinanceTableSection, TableBySKU, false);
				subheader.FindViewById<TextView>(Resource.Id.ftsTitle).Text = SKU.name;;
				TableBySKU.AddView(subheader);
				foreach (var type in infoTypes) {
					var row = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.FinanceTableItem, TableBySKU, false);
					row.FindViewById<TextView>(Resource.Id.ftiDrugSKUTV).Text = GetTypeName(type);
					for (int v = 1; v <= dates.Count(); v++) {
						dict_key = string.Format("{0}-{1}-{2}", SKU.uuid, type, dates[v - 1].ToString(PeriodFormatForKey));
						var view = row.GetChildAt(v);
						if (view is TextView) {
							(view as TextView).Text = string.Empty;
							TextViewsBySKU.Add(dict_key, (view as TextView));
						}
					}
					TableBySKU.AddView(row);
				}
			}

			generateViews.Stop();

			Console.WriteLine(
				"FinanceData: RefreshViewBySKU={0}-{1}",
				generateViews.ElapsedMilliseconds,
				TextViewsBySKU.Count);
			
			// 2. Вставляем данные
			SetValues(financeDatas);

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

