﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using Android.App;
using Android.OS;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite
{
	[Activity(Label = "TestDataActivity")]
	public class TestDataActivity : Activity
	{
		Button GenerateData;
		Button CustomAction;
		Button Clear;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.TestData);

			GenerateData = FindViewById<Button>(Resource.Id.tdaGenerateDataB);

			GenerateData.Click += GenerateData_Click;

			CustomAction = FindViewById<Button>(Resource.Id.tdaCustomActionB);

			CustomAction.Click += CustomAction_Click;

			Clear = FindViewById<Button>(Resource.Id.tdaClearB);
			Clear.Click += Clear_Click;

			RefreshView();
		}

		void Clear_Click(object sender, EventArgs e)
		{
			var transaction = MainDatabase.BeginTransaction();

			MainDatabase.DeleteAttendancies();
			MainDatabase.DeleteDistributions();
			MainDatabase.DeleteFinanceData();
			MainDatabase.DeleteSaleData();

			MainDatabase.DeleteItems<CoterieData>();
			MainDatabase.DeleteItems<PresentationData>();
			MainDatabase.DeleteItems<Employee>();
			MainDatabase.DeleteItems<PromotionData>();
			MainDatabase.DeleteItems<CompetitorData>();
			MainDatabase.DeleteItems<MessageData>();
			MainDatabase.DeleteItems<ResumeData>();
			MainDatabase.DeleteItems<PhotoData>();
			MainDatabase.DeleteItems<Material>();
			MainDatabase.DeleteItems<ListedHospital>();

			transaction.Commit();
		}

		void CustomAction_Click(object sender, EventArgs e)
		{
			//using (var transaction = MainDatabase.BeginTransaction()) {
			//	MainDatabase.DeleteItems<FinanceDataByMonth>();
			//	MainDatabase.DeleteItems<FinanceDataByQuarter>();
			//	transaction.Commit();
			//}
			string UUID = @"Pharmcay#" + 237;//string.Empty;
			//var rnd = new Random();
			//rnd.Next();
			//var gen = new Stopwatch();
			//gen.Start();
			//var SKUs = MainDatabase.GetItems<DrugSKU>();
			//var trans = MainDatabase.BeginTransaction();
			//for (int i = 0; i < 500; i++) {
			//	var uuid = @"Pharmcay#" + i;
			//	foreach (var sku in SKUs) {
			//		for (int m = 1; m <= 12; m++) {
			//			var financeData = MainDatabase.Create<FinanceDataByMonth>();
			//			financeData.Pharmacy = uuid;
			//			financeData.DrugSKU = sku.uuid;
			//			financeData.Year = 2016;
			//			financeData.Month = m;
			//			financeData.Sale = rnd.NextDouble() < 0.2 ? null : (float?)(rnd.NextDouble() * 60);
			//			financeData.Purchase = rnd.NextDouble() < 0.2 ? null : (float?)(rnd.NextDouble() * 40);
			//			financeData.Remain = rnd.NextDouble() < 0.2 ? null : (float?)(rnd.NextDouble() * 20);
			//		}	
			//	}
			//}
			//trans.Commit();
			//gen.Stop();
			//Console.WriteLine(@"Gen: {0}", gen.ElapsedMilliseconds);

			var calc = new Stopwatch();
			calc.Start();
			var datas = MainDatabase.GetPharmacyDatas<FinanceDataByMonth>(UUID);
			// datas.OrderBy(d => d.DrugSKU).ThenBy(d => d.Year).ThenBy(d =>d.Month
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

			var oldQuarters = MainDatabase.GetPharmacyDatas<FinanceDataByQuarter>(UUID);
			var trans = MainDatabase.BeginTransaction(); 
			foreach (var sku in dict) {
				foreach (var year in sku.Value) {
					for (int q = 1; q <= 4; q++) {
						if (year.Value[q - 1].Count == 3) {
							if (oldQuarters.SingleOrDefault(oq => oq.DrugSKU == sku.Key && oq.Year == year.Key && oq.Quarter == q) == null) {
								var quarter = MainDatabase.Create<FinanceDataByQuarter>();
								quarter.Pharmacy = UUID;
								quarter.DrugSKU = sku.Key;
								quarter.Year = year.Key;
								quarter.Quarter = q;
								foreach (var item in year.Value[q - 1]) {
									quarter.Sale += item.Sale;
									quarter.Purchase += item.Purchase;
									quarter.Remain += item.Remain;
								}
							}
						}
					}
				}
			}
			trans.Commit();
			calc.Stop();
			Console.WriteLine(@"Calc: {0}, Count: {1}, oldQuarters: {2}", calc.ElapsedMilliseconds, datas.Count, oldQuarters.Count);
		}

		void GenerateData_Click(object sender, EventArgs e)
		{
			var rnd = new Random();

			var step = 7;
			var visitsCount = 10;
			var baseDate = DateTimeOffset.Now.AddDays(-1 * visitsCount * step);

			var promotions = MainDatabase.GetItems<Promotion>();
			var drugSKUs = MainDatabase.GetDrugSKUs();
			var pharmacies = MainDatabase.GetItems<Pharmacy>();
			var transaction = MainDatabase.BeginTransaction();

			MainDatabase.DeleteAttendancies();
			MainDatabase.DeleteDistributions();
			MainDatabase.DeleteFinanceData();
			MainDatabase.DeleteSaleData();

			MainDatabase.DeleteItems<CoterieData>();
			MainDatabase.DeleteItems<PresentationData>();
			MainDatabase.DeleteItems<Employee>();
			MainDatabase.DeleteItems<PromotionData>();
			MainDatabase.DeleteItems<CompetitorData>();
			MainDatabase.DeleteItems<MessageData>();
			MainDatabase.DeleteItems<ResumeData>();
			MainDatabase.DeleteItems<PhotoData>();

			transaction.Commit();

			transaction = MainDatabase.BeginTransaction();

			foreach (var pharmacy in pharmacies) {
				// Визиты
				if (true) {
					for (int i = 0; i < visitsCount; i++) {
						var attendance = MainDatabase.Create<Attendance>();
						attendance.Pharmacy = pharmacy.UUID;
						attendance.When = baseDate.AddDays(i * step);

						foreach (var SKU in drugSKUs) {
							var distribution = MainDatabase.CreateData<DistributionData>(attendance.UUID);
							distribution.DrugSKU = SKU.uuid;
							distribution.IsExistence = rnd.NextDouble() > 0.5;
							distribution.Count = rnd.Next(0, 10);
							distribution.Price = (int)(rnd.NextDouble() * 200);
							distribution.IsPresence = rnd.NextDouble() > 0.5;
							distribution.HasPOS = rnd.NextDouble() > 0.5;
							distribution.Order = @"Order " + SKU.uuid;
							distribution.Comment = @"Comment " + SKU.uuid;
						}
					}
				}

				// Продажи
				var financesCount = 5;
				baseDate = DateTimeOffset.Now.AddMonths(-3);
				for (int i = 0; i < financesCount; i++) {
					foreach (var SKU in drugSKUs) {
						var financeData = MainDatabase.Create<FinanceData>();
						financeData.Pharmacy = pharmacy.UUID;
						financeData.Period = baseDate.AddMonths(i);
						financeData.DrugSKU = SKU.uuid;
						financeData.Sale = rnd.NextDouble() > 0.3 ? null : (float?)(rnd.NextDouble() * 20);
						financeData.Purchase = rnd.NextDouble() > 0.4 ? null : (float?)(rnd.NextDouble() * 20);
						financeData.Remain = rnd.NextDouble() > 0.5 ? null : (float?)(rnd.NextDouble() * 20);
					}
				}

				// Содержание визите = Презентаци
				int employeesCount = 3;
				//int presentationsCount = 5;
				var attendanceLast = MainDatabase.GetAttendaces(pharmacy.UUID).OrderByDescending(i => i.When).FirstOrDefault();
				var positions = MainDatabase.GetItems<Position>();
				var workTypes = MainDatabase.GetItems<WorkType>();
				var brands = MainDatabase.GetItems<DrugBrand>();
				var employees = MainDatabase.GetEmployees(pharmacy.UUID);

				if (employees.Count == 0) {
					for (int emp = 0; emp < employeesCount; emp++) {
						var employee = MainDatabase.CreateEmployee(pharmacy.UUID);
						employee.SetSex(Sex.Male);
						employee.Name = (rnd.NextDouble() > 0.5 ? @"Иванов" : @"Петров") + @" "
									  + (rnd.NextDouble() > 0.5 ? @"Иван" : @"Петр") + @" "
									  + (rnd.NextDouble() > 0.5 ? @"Иванович" : @"Петрович");
						employee.Position = positions[rnd.Next(positions.Count)].uuid;
						employee.BirthDate = DateTimeOffset.Now.AddYears(-rnd.Next(25, 40)).AddDays((rnd.NextDouble() - 0.5) * 365);
						employees.Add(employee);
					}
				}

				foreach(var employee in employees) {
					foreach (var brand in brands) {
						if (rnd.NextDouble() > 0.5) continue;
							
						for (int wt = 0; wt < workTypes.Count; wt++) {
							if (rnd.NextDouble() > 0.5) continue;

							var presentationData = MainDatabase.CreateData<PresentationData>(attendanceLast.UUID);
							presentationData.Employee = employee.UUID;
							presentationData.Brand = brand.uuid;
							presentationData.WorkType = workTypes[wt].uuid;
						}
					}
				}

				// Содержание визите = Фарм-кружок
				foreach (var employee in employees) {
					if (rnd.NextDouble() > 0.5) continue;

					foreach (var brand in brands) {
						if (rnd.NextDouble() > 0.5) continue;

						var coterieData = MainDatabase.CreateData<CoterieData>(attendanceLast.UUID);
						coterieData.Employee = employee.UUID;
						coterieData.Brand = brand.uuid;
					}
				}

				// Акция
				var promotionData = MainDatabase.CreateData<PromotionData>(attendanceLast.UUID);
				promotionData.Promotion = promotions[rnd.Next(promotions.Count - 1)].uuid;
				promotionData.Text = @"promotionData";

				// Активность конкурентов
				var competitorData = MainDatabase.CreateData<CompetitorData>(attendanceLast.UUID);
				competitorData.Text = @"competitorData";

				// Сообщения от аптеки
				var messageTypes = MainDatabase.GetItems<MessageType>();
				foreach (var item in messageTypes) {
					if (rnd.NextDouble() > 0.5) continue;

					var messageData = MainDatabase.CreateData<MessageData>(attendanceLast.UUID);
					messageData.Type = item.uuid;
					messageData.Text = @"messageData " + item.uuid;
				}

				// Резюме визита;
				var resumeData = MainDatabase.CreateData<ResumeData>(attendanceLast.UUID);
				resumeData.Text = @"resumeData";
			}

			transaction.Commit();

			RefreshView();
		}

		void RefreshView()
		{
			FindViewById<TextView>(Resource.Id.tdaPharmacies).Text = string.Format(@"Pharmacies: {0}", MainDatabase.GetItems<Pharmacy>().Count);
			FindViewById<TextView>(Resource.Id.tdaAttendances).Text = string.Format(@"Attendances: {0}", MainDatabase.GetItems<Attendance>().Count);
			FindViewById<TextView>(Resource.Id.tdaDistributions).Text = string.Format(@"Distributions: {0}", MainDatabase.GetItems<DistributionData>().Count);
			FindViewById<TextView>(Resource.Id.tdaFinanceDatas).Text = string.Format(@"FinanceDatas: {0}", MainDatabase.GetItems<FinanceData>().Count);
			FindViewById<TextView>(Resource.Id.tdaPresentationDatas).Text = string.Format(@"PresentationDatas: {0}", MainDatabase.GetItems<PresentationData>().Count);
			FindViewById<TextView>(Resource.Id.tdaCoteriaDatas).Text = string.Format(@"CoteriaDatas: {0}", MainDatabase.GetItems<CoterieData>().Count);
			FindViewById<TextView>(Resource.Id.tdaPromotionDatas).Text = string.Format(@"PromotionDatas: {0}", MainDatabase.GetItems<PromotionData>().Count); 
			FindViewById<TextView>(Resource.Id.tdaCompetitorDatas).Text = string.Format(@"CompetitorDatas: {0}", MainDatabase.GetItems<CompetitorData>().Count);
			FindViewById<TextView>(Resource.Id.tdaMessageDatas).Text = string.Format(@"MessageDatas: {0}", MainDatabase.GetItems<MessageData>().Count);
			FindViewById<TextView>(Resource.Id.tdaResumeDatas).Text = string.Format(@"ResumeDatas: {0}", MainDatabase.GetItems<ResumeData>().Count);
			FindViewById<TextView>(Resource.Id.tdaSaleDatas).Text = string.Format(@"SaleDatas: {0}", MainDatabase.GetItems<SaleData>().Count);
			FindViewById<TextView>(Resource.Id.tdaPhotoDatas).Text = string.Format(@"PhotoDatas: {0}", MainDatabase.GetItems<PhotoData>().Count);

		}
	}
}

