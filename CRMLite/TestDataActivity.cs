
using System;
using System.Collections.Generic;
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
	[Activity(Label = "TestDataActivity")]
	public class TestDataActivity : Activity
	{
		Button GenerateData;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.TestData);

			GenerateData = FindViewById<Button>(Resource.Id.tdaGenerateDataB);

			GenerateData.Click += GenerateData_Click;

			RefreshView();
		}

		void GenerateData_Click(object sender, EventArgs e)
		{
			var baseDate = DateTimeOffset.Now;
			var step = 7;
			var visitsCount = 20;
			var financesCount = 5;

			var drugSKUs = MainDatabase.GetDrugSKUs();
			var pharmacies = MainDatabase.GetItems<Pharmacy>();
			var transaction = MainDatabase.BeginTransaction();
			foreach (var pharmacy in pharmacies) {
				// Визиты
				if (false) {
					for (int i = 0; i < visitsCount; i++) {
						var attendance = MainDatabase.Create<Attendance>();
						attendance.Pharmacy = pharmacy.UUID;
						attendance.When = baseDate.AddDays(i * step);

						foreach (var SKU in drugSKUs) {
							var distribution = MainDatabase.CreateData<Distribution>(attendance.UUID);
							var rnd = new Random();
							distribution.DrugSKU = SKU.uuid;
							distribution.IsExistence = rnd.NextDouble() > 0.5;
							distribution.Count = rnd.Next(0, 10);
							distribution.Price = (float)(rnd.NextDouble() * 200);
							distribution.IsPresence = rnd.NextDouble() > 0.5;
							distribution.HasPOS = rnd.NextDouble() > 0.5;
							distribution.Order = @"Order " + SKU.uuid;
							distribution.Comment = @"Comment " + SKU.uuid;
						}
					}
				}

				MainDatabase.DeleteFinanceData();
				// Продажи
				baseDate = DateTimeOffset.Now.AddMonths(-3);
				for (int i = 0; i < financesCount; i++) {
					foreach (var SKU in drugSKUs) {
						var financeData = MainDatabase.Create<FinanceData>();
						financeData.Pharmacy = pharmacy.UUID;
						financeData.Period = baseDate.AddMonths(i);
						var rnd = new Random();
						financeData.DrugSKU = SKU.uuid;
						financeData.Sale = rnd.NextDouble() > 0.3 ? null : (float?)(rnd.NextDouble() * 20);
						financeData.Purchase = rnd.NextDouble() > 0.4 ? null : (float?)(rnd.NextDouble() * 20); ;
						financeData.Remain = rnd.NextDouble() > 0.5 ? null : (float?)(rnd.NextDouble() * 20); ;
					}
				}
			}

			transaction.Commit();

			RefreshView();
		}

		void RefreshView()
		{
			FindViewById<TextView>(Resource.Id.tdaPharmacies).Text = string.Format(@"Pharmacies: {0}", MainDatabase.GetItems<Pharmacy>().Count);
			FindViewById<TextView>(Resource.Id.tdaAttendances).Text = string.Format(@"Attendances: {0}", MainDatabase.GetItems<Attendance>().Count);
			FindViewById<TextView>(Resource.Id.tdaDistributions).Text = string.Format(@"Distributions: {0}", MainDatabase.GetItems<Distribution>().Count);
			FindViewById<TextView>(Resource.Id.tdaFinanceDatas).Text = string.Format(@"FinanceDatas: {0}", MainDatabase.GetItems<FinanceData>().Count);

		}
	}
}

