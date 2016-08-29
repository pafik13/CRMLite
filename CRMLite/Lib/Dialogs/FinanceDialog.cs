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

namespace CRMLite.Dialogs
{
	public class FinanceDialog : DialogFragment
	{

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

		Spinner Position;
		List<Position> positions;

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

			#region DrugSKU
			var drugSKUs = new List<DrugSKU>();
			drugSKUs.Add(new DrugSKU { name = @"Выберите SKU!", uuid = Guid.Empty.ToString() });
			drugSKUs.AddRange(MainDatabase.GetItems<DrugSKU>());

			var drugSKU = view.FindViewById<Spinner>(Resource.Id.fdDrugSKUS);
			var drugSKUAdapter = new ArrayAdapter(
				Activity,
				Android.Resource.Layout.SimpleSpinnerItem,
				drugSKUs.Select(x => x.name).ToArray()
			);
			drugSKUAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			drugSKU.Adapter = drugSKUAdapter;
			//drugSKU.ItemSelected += (sender, e) => {
			//	if (e.Position == 0) {
			//		FinanceData.DrugSKU = null;
			//	} else {
			//		FinanceData.DrugSKU = drugSKUs[e.Position].uuid;
			//	}
			//};
			#endregion

			#region PeriodType
			var periodTypes = new string[7];
			periodTypes[0] = @"Выберите период!";
			for (int p = 1; p < 7; p++) {
				periodTypes[p] = string.Format(@"{0} месяц(а)", p);
			}

			var periodType = view.FindViewById<Spinner>(Resource.Id.fdPeriodTypeS);
			var periodTypeAdapter = new ArrayAdapter(
				Activity,
				Android.Resource.Layout.SimpleSpinnerItem,
				periodTypes
			);
			periodTypeAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			periodType.Adapter = periodTypeAdapter;
			// 			periodType.ItemSelected += (sender, e) =>
			// 			{
			// 				if (e.Position == 0) {
			// 					FinanceData.Period = null;
			// 				}
			// 				else {
			// 					FinanceData.Period = drugSKUs[e.Position].uuid;
			// 				}
			// 			};
			#endregion

			#region StartMonth
			var startMonths = new string[26]; // -12 0 +12 = 25 и +1[Выберите]
			startMonths[0] = @"Выберите месяц!";
			for (int p = 1; p < 26; p++) {
				startMonths[p] = DateTimeOffset.Now.AddMonths(-12 + p - 1).ToString(@"MMMM - yyyy");
			}

			var startMonth = view.FindViewById<Spinner>(Resource.Id.fdStartMonthS);
			var startMonthAdapter = new ArrayAdapter(
				Activity,
				Android.Resource.Layout.SimpleSpinnerItem,
				startMonths
			);
			startMonthAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			startMonth.Adapter = startMonthAdapter;
			// 			startMonth.ItemSelected += (sender, e) =>
			// 			{
			// 				if (e.Position == 0) {
			// 					FinanceData.Period = null;
			// 				}
			// 				else {
			// 					FinanceData.Period = drugSKUs[e.Position].uuid;
			// 				}
			// 			};
			#endregion

			//view.FindViewById<EditText>(Resource.Id.fdSaleET).Text = FinanceData.Sale == null ? string.Empty : FinanceData.Sale.ToString();
			//view.FindViewById<EditText>(Resource.Id.fdPurchaseET).Text = FinanceData.Purchase == null ? string.Empty : FinanceData.Sale.ToString();
			//view.FindViewById<EditText>(Resource.Id.fdRemainET).Text = FinanceData.Remain == null ? string.Empty : FinanceData.Sale.ToString();

			view.FindViewById<Button>(Resource.Id.fdCloseB).Click += delegate {
				Dismiss();
			};

			view.FindViewById<Button>(Resource.Id.fdSaveB).Click += delegate {
				Toast.MakeText(Activity, "SAVE BUTTON CLICKED", ToastLength.Short).Show();

				var financeDatas = new List<FinanceData>();

				if ((periodType.SelectedItemPosition > 0) 
				    && (drugSKU.SelectedItemPosition >0)
				    && (startMonth.SelectedItemPosition > 0))
				{
					Transaction = MainDatabase.BeginTransaction();

					for (int m = 0; m < periodType.SelectedItemPosition; m++) {
						var financeData = MainDatabase.Create<FinanceData>();
						//var financeData = new FinanceData();
						financeData.Pharmacy = Pharmacy.UUID;
						financeData.DrugSKU = drugSKUs[drugSKU.SelectedItemPosition].uuid;
						financeData.Period = DateTimeOffset.Now.AddMonths(-12 + startMonth.SelectedItemPosition - 1).AddMonths(m);
						financeData.Sale = ConvertToFloat(view.FindViewById<EditText>(Resource.Id.fdSaleET).Text, periodType.SelectedItemPosition);
						financeData.Purchase = ConvertToFloat(view.FindViewById<EditText>(Resource.Id.fdPurchaseET).Text, periodType.SelectedItemPosition);
						financeData.Remain = ConvertToFloat(view.FindViewById<EditText>(Resource.Id.fdRemainET).Text, periodType.SelectedItemPosition);
						financeDatas.Add(financeData);
					}

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

				OnAfterSaved(new AfterSavedEventArgs(financeDatas));

				Dismiss();
			};

			return view;
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

