using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using Realms;

using CRMLite.Entities;
using System.Globalization;
using System.Diagnostics;

namespace CRMLite.Dialogs
{
	public class FinanceDialog : DialogFragment
	{
		public const string TAG = @"FinanceDialog";

		public event EventHandler<AfterSavedEventArgs> AfterSaved;

		public class AfterSavedEventArgs : EventArgs
		{
			public AfterSavedEventArgs(List<FinanceData> financeDatas)
			{
				FinanceDatas = financeDatas;
			}

			public List<FinanceData> FinanceDatas { get; private set; }
		}

		Pharmacy Pharmacy;
		Transaction Transaction;
		FinanceData FinanceData;
		IList<DrugSKU> SKUs;
		LinearLayout FinanceTable;

		protected virtual void OnAfterSaved(AfterSavedEventArgs e)
		{
			if (AfterSaved != null) {
				AfterSaved(this, e);
			}
		}

		public FinanceDialog(Pharmacy pharmacy, FinanceData financeData = null)
		{
			if (pharmacy == null) {
				throw new ArgumentNullException(nameof(pharmacy));
			}
			Pharmacy = pharmacy;
			FinanceData = financeData;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			Dialog.SetCanceledOnTouchOutside(false);

			var caption = string.Empty;
			if (FinanceData == null) {
				caption = "НОВАЯ ФИН. ИНФОРМАЦИЯ";
				// FinanceData = MainDatabase.Create<FinanceData>();
			}

			Dialog.SetTitle(caption);

			View view = inflater.Inflate(Resource.Layout.FinanceDialog, container, false);


			#region StartMonth
			//var months = new List<DateTimeOffset>();
			//for (int m = 0; m < 17; m++) { // -12 0 +3 = 16 и +1[Выберите]
			//	months.Add(DateTimeOffset.Now.AddMonths(-12 + m - 1));
			//}

			var months = new string[17]; // -12 0 +3 = 16 и +1[Выберите]
			months[0] = @"Выберите месяц!";
			for (int m = 1; m < 17; m++) {
				months[m] = DateTimeOffset.Now.AddMonths(-12 + m - 1).ToString(@"MMMM - yyyy");
			}

			var startMonth = view.FindViewById<Spinner>(Resource.Id.fdStartMonthS);
			var startMonthAdapter = new ArrayAdapter(
				Activity,
				Android.Resource.Layout.SimpleSpinnerItem,
				months
			);
			startMonthAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			startMonth.Adapter = startMonthAdapter;
			startMonth.ItemSelected += (sender, e) => {
				if (e.Position == 0) {
					for (int c = 2; c < FinanceTable.ChildCount; c = c + 2) {
						var row = (LinearLayout)FinanceTable.GetChildAt(c);
						for (int cc = 1; cc < row.ChildCount; cc++) {
							row.GetChildAt(cc).Enabled = false;
						}
					}
				} else {
					for (int c = 2; c < FinanceTable.ChildCount; c = c + 2) {
						var row = (LinearLayout)FinanceTable.GetChildAt(c);
						for (int cc = 1; cc < row.ChildCount; cc++) {
							row.GetChildAt(cc).Enabled = true;
						}
					}
				}
			};
			#endregion

			FinanceTable = view.FindViewById<LinearLayout>(Resource.Id.fdFinanceTable);
			var header = (LinearLayout)inflater.Inflate(Resource.Layout.FinanceDialogTableHeader, FinanceTable, false);
			var divider = inflater.Inflate(Resource.Layout.Divider, FinanceTable, false);

			FinanceTable.AddView(header);
			FinanceTable.AddView(divider);

			SKUs = MainDatabase.GetItems<DrugSKU>();
			foreach (var SKU in SKUs) {
				var row = (inflater.Inflate(
									Resource.Layout.FinanceDialogTableItem,
									FinanceTable,
									false)) as LinearLayout;
				row.SetTag(Resource.String.DrugSKUUUID, SKU.uuid);
				row.FindViewById<TextView>(Resource.Id.fdtiDrugSKUTV).Text = SKU.name;

				var sale = row.FindViewById<EditText>(Resource.Id.fdtiSaleET);
				sale.SetTag(Resource.String.IsChanged, false);
				sale.AfterTextChanged += RView_AfterTextChanged;

				var purchase = row.FindViewById<EditText>(Resource.Id.fdtiPurchaseET);
				purchase.SetTag(Resource.String.IsChanged, false);
				purchase.AfterTextChanged += RView_AfterTextChanged;

				var remain = row.FindViewById<EditText>(Resource.Id.fdtiRemainET);
				remain.SetTag(Resource.String.IsChanged, false);
				remain.AfterTextChanged += RView_AfterTextChanged;

				FinanceTable.AddView(row);

				divider = inflater.Inflate(Resource.Layout.Divider, FinanceTable, false);

				FinanceTable.AddView(divider);
			}


			view.FindViewById<Button>(Resource.Id.fdCloseB).Click += delegate {
				Dismiss();
			};

			view.FindViewById<Button>(Resource.Id.fdSaveB).Click += delegate {
				//Toast.MakeText(Activity, "SAVE BUTTON CLICKED", ToastLength.Short).Show();
				var financeDatas = new List<FinanceData>();

				if (startMonth.SelectedItemPosition > 0)
				{
					var period = DateTimeOffset.Now.AddMonths(-12 + startMonth.SelectedItemPosition - 1);
					Transaction = MainDatabase.BeginTransaction();

					for (int c = 2; c < FinanceTable.ChildCount; c = c + 2) {
						var row = (LinearLayout)FinanceTable.GetChildAt(c);

						var sale = Helper.ToFloat(row.FindViewById<EditText>(Resource.Id.fdtiSaleET).Text);
						var purchase = Helper.ToFloat(row.FindViewById<EditText>(Resource.Id.fdtiPurchaseET).Text);
						var remain = Helper.ToFloat(row.FindViewById<EditText>(Resource.Id.fdtiRemainET).Text);

						if (sale == null && purchase == null && remain == null) continue;

						var financeData = MainDatabase.Create<FinanceDataByMonth>();
						financeData.Pharmacy = Pharmacy.UUID;
						financeData.DrugSKU = (string)row.GetTag(Resource.String.DrugSKUUUID);
						financeData.Year = period.Year;
						financeData.Month = period.Month;
						financeData.Sale = sale;
						financeData.Purchase = purchase;
						financeData.Remain = remain;
					}

					// 6.1 Собираем данные в кварталы
					var calcQuraters = new Stopwatch();
					calcQuraters.Start();
					var datas = MainDatabase.GetPharmacyDatas<FinanceDataByMonth>(Pharmacy.UUID);
					var dict = new Dictionary<string, Dictionary<int, List<FinanceDataByMonth>[]>>();
					foreach (var item in datas) {
						if (dict.ContainsKey(item.DrugSKU)) {
							if (dict[item.DrugSKU].ContainsKey(item.Year)) {
								dict[item.DrugSKU][item.Year][(item.Month - 1) / 3].Add(item);
							} else {
								dict[item.DrugSKU].Add(item.Year, new List<FinanceDataByMonth>[4]);
								dict[item.DrugSKU][item.Year][0] = new List<FinanceDataByMonth>();
								dict[item.DrugSKU][item.Year][1] = new List<FinanceDataByMonth>();
								dict[item.DrugSKU][item.Year][2] = new List<FinanceDataByMonth>();
								dict[item.DrugSKU][item.Year][3] = new List<FinanceDataByMonth>();
								dict[item.DrugSKU][item.Year][(item.Month - 1) / 3].Add(item);
							}
						} else {
							dict.Add(item.DrugSKU, new Dictionary<int, List<FinanceDataByMonth>[]>());
							dict[item.DrugSKU].Add(item.Year, new List<FinanceDataByMonth>[4]);
							dict[item.DrugSKU][item.Year][0] = new List<FinanceDataByMonth>();
							dict[item.DrugSKU][item.Year][1] = new List<FinanceDataByMonth>();
							dict[item.DrugSKU][item.Year][2] = new List<FinanceDataByMonth>();
							dict[item.DrugSKU][item.Year][3] = new List<FinanceDataByMonth>();
							dict[item.DrugSKU][item.Year][(item.Month - 1) / 3].Add(item);
						}
					}

					var oldQuarters = MainDatabase.GetPharmacyDatas<FinanceDataByQuarter>(Pharmacy.UUID);
					int newQuarters = 0;
					foreach (var sku in dict) {
						foreach (var year in sku.Value) {
							for (int q = 1; q <= 4; q++) {
								if (year.Value[q - 1].Count == 3) {
									newQuarters++;
									var oldQuarter = oldQuarters.SingleOrDefault(oq => oq.DrugSKU == sku.Key && oq.Year == year.Key && oq.Quarter == q);
									if (oldQuarter == null) {
										var quarter = MainDatabase.Create<FinanceDataByQuarter>();
										quarter.Pharmacy = Pharmacy.UUID;
										quarter.DrugSKU = sku.Key;
										quarter.Year = year.Key;
										quarter.Quarter = q;
										quarter.Sale = 0.0f;
										quarter.Purchase = 0.0f;
										quarter.Remain = 0.0f;
										foreach (var item in year.Value[q - 1]) {
											quarter.Sale += item.Sale;
											quarter.Purchase += item.Purchase;
											quarter.Remain += item.Remain;
										}
										if (quarter.Sale.HasValue) { 
											if (Math.Abs(quarter.Sale.Value) < 0.01) quarter.Sale = null; 
										}
										if (quarter.Purchase.HasValue) {
											if (Math.Abs(quarter.Purchase.Value) < 0.01) quarter.Purchase = null;
										}
										if (quarter.Remain.HasValue) { 
											if (Math.Abs(quarter.Remain.Value) < 0.01) quarter.Remain = null; 
										}

									} else {
										oldQuarter.Sale = 0.0f;
										oldQuarter.Purchase = 0.0f;
										oldQuarter.Remain = 0.0f;
										foreach (var item in year.Value[q - 1]) {
											oldQuarter.Sale += item.Sale;
											oldQuarter.Purchase += item.Purchase;
											oldQuarter.Remain += item.Remain;
										}
										if (oldQuarter.Sale.HasValue) {
											if (Math.Abs(oldQuarter.Sale.Value) < 0.01) oldQuarter.Sale = null;
										}
										if (oldQuarter.Purchase.HasValue) {
											if (Math.Abs(oldQuarter.Purchase.Value) < 0.01) oldQuarter.Purchase = null;
										}
										if (oldQuarter.Remain.HasValue) {
											if (Math.Abs(oldQuarter.Remain.Value) < 0.01) oldQuarter.Remain = null;
										}
									}
								}
							}
						}
					}
					calcQuraters.Stop();
					Console.WriteLine(@"Calc: {0}, Count: {1}, oldQuarters: {2}, newQuarters: {3}", calcQuraters.ElapsedMilliseconds, datas.Count, oldQuarters.Count, newQuarters);


					Transaction.Commit();
				}
				// var sync = new SyncItem()
				// {
				// 	Path = @"Employee",
				// 	ObjectUUID = employee.UUID,
				// 	JSON = JsonConvert.SerializeObject(employee)
				// };

				// MainDatabase.AddToQueue(sync);

				// context.StartService(new Intent("com.xamarin.SyncService"));
				//var financeDatas = new List<FinanceData>();
				OnAfterSaved(new AfterSavedEventArgs(financeDatas));

				Dismiss();
			};

			return view;
		}

		void RView_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
		{
			var editText = sender as EditText;
			editText.SetTag(Resource.String.IsChanged, true);
		}

		float? ConvertToFloat(string value, int divider = 1)
		{
			if (string.IsNullOrEmpty(value)) return null;

			float result;
			float.TryParse(value.Replace(',', '.'), NumberStyles.Float, new CultureInfo("en-US").NumberFormat, out result);

			if (float.IsInfinity(result) || result == 0.0f) return null;

			result /= divider;

			if (float.IsInfinity(result) || result == 0.0f) return null;

			return result;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Pharmacy = null;
			FinanceData = null;
		}
	}
}

