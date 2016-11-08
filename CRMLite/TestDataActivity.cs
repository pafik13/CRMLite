using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using SDiag = System.Diagnostics;

using Android.App;
using Android.OS;
using Android.Widget;

using CRMLite.Entities;
using CRMLite.Lib.Sync;
using RestSharp;
using System.IO;
using Android.Content;
using Android.Accounts;
using CRMLite.Dialogs;

namespace CRMLite
{
	[Activity(Label = "TestDataActivity")]
	public class TestDataActivity : Activity
	{
		const int PICKFILE_REQUEST_CODE = 3;

		Button GenerateData;
		Button CustomAction;
		Button Clear;
		Button ChangeWorkMode;

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

			ChangeWorkMode = FindViewById<Button>(Resource.Id.tdaChangeWorkModeB);
			ChangeWorkMode.Click += (sender, e) => {
				switch (Helper.WorkMode) {
					case WorkMode.wmOnlyRoute:
						Helper.WorkMode = WorkMode.wmRouteAndRecommendations;
						break;
					case WorkMode.wmRouteAndRecommendations:
						Helper.WorkMode = WorkMode.wmOnlyRecommendations;
						break;
					case WorkMode.wmOnlyRecommendations:
						Helper.WorkMode = WorkMode.wmOnlyRoute;
						break;
				}
			};

			RefreshView();
		}

		void Clear_Click(object sender, EventArgs e)
		{
			//var transaction = MainDatabase.BeginTransaction();

			//MainDatabase.DeleteAttendancies();
			//MainDatabase.DeleteDistributions();
			//MainDatabase.DeleteFinanceData();
			//MainDatabase.DeleteSaleData();

			//MainDatabase.DeleteItems<FinanceDataByMonth>();
			//MainDatabase.DeleteItems<FinanceDataByQuarter>();
			//MainDatabase.DeleteItems<SaleDataByMonth>();
			//MainDatabase.DeleteItems<SaleDataByQuarter>();

			//MainDatabase.DeleteItems<CoterieData>();
			//MainDatabase.DeleteItems<PresentationData>();
			//MainDatabase.DeleteItems<Employee>();
			//MainDatabase.DeleteItems<PromotionData>();
			//MainDatabase.DeleteItems<CompetitorData>();
			//MainDatabase.DeleteItems<MessageData>();
			//MainDatabase.DeleteItems<ResumeData>();
			//MainDatabase.DeleteItems<PhotoData>();
			//MainDatabase.DeleteItems<Material>();
			//MainDatabase.DeleteItems<ListedHospital>();

			//MainDatabase.DeleteItems<RouteItem>();


			//transaction.Commit();

			MainDatabase.ClearDB();
			MainDatabase.Dispose();
		}

		public Account CreateSyncAccount(Context context)
		{
			var newAccount = new Account(SyncConst.ACCOUNT, SyncConst.ACCOUNT_TYPE);

			var accountManager = (AccountManager)context.GetSystemService(AccountService);

			if (accountManager.AddAccountExplicitly(newAccount, null, null)) {
				SDiag.Debug.WriteLine("AddAccountExplicitly");
			} else {
				SDiag.Debug.WriteLine("NOT AddAccountExplicitly");
			}

			return newAccount;
		}

		void CustomAction_Click(object sender, EventArgs e)
		{
			new Thread(() => {
				var DB = Realms.Realm.GetInstance(MainDatabase.DBPath);

				//var ojb = new Distributor()
				var json = "{\"name\":\"NewDistr3\",\"uuid\":\"2817a473-9720-424d-b379-3cedd7b90c80\",\"createdAt\":\"2016-11-07T18:50:58.960Z\",\"updatedAt\":\"2016-11-07T18:50:58.960Z\",\"id\":\"5820cd12e53164e74abff8f9\"}"; ;
				Realms.RealmObject ojb = Newtonsoft.Json.JsonConvert.DeserializeObject<Distributor>(json);
				using (var trans = DB.BeginWrite()) {

					DB.Manage(ojb);
					trans.Rollback();
				}
			}).Start();
			//ojb.
			//var  = DB.CreateObject<Distributor>();

			//ru.sbl.crmlite2

			//var account = CreateSyncAccount(this);

			//var settingsBundle = new Bundle();
			////settingsBundle.PutBoolean(ContentResolver.SyncExtrasManual, true);
			////settingsBundle.PutBoolean(ContentResolver.SyncExtrasExpedited, true);
			//var shared = GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private);

			//var ACCESS_TOKEN = shared.GetString(SigninDialog.C_ACCESS_TOKEN, string.Empty);
			//var HOST_URL = shared.GetString(SigninDialog.C_HOST_URL, string.Empty);

			//settingsBundle.PutString(SigninDialog.C_ACCESS_TOKEN, ACCESS_TOKEN);
			//settingsBundle.PutString(SigninDialog.C_HOST_URL, HOST_URL);
			//settingsBundle.PutBoolean(ContentResolver.SyncExtrasExpedited, false);
			//settingsBundle.PutBoolean(ContentResolver.SyncExtrasDoNotRetry, false);
   //     	settingsBundle.PutBoolean(ContentResolver.SyncExtrasManual, false);
			////ContentResolver.RequestSync(account, SyncConst.AUTHORITY, settingsBundle);

			////ContentResolver.AddPeriodicSync(account, SyncConst.AUTHORITY, Bundle.Empty, SyncConst.SYNC_INTERVAL)
			//ContentResolver.SetIsSyncable(account, SyncConst.AUTHORITY, 1);
			//ContentResolver.SetSyncAutomatically(account, SyncConst.AUTHORITY, true);
			//;
			//ContentResolver.AddPeriodicSync(account, SyncConst.AUTHORITY, settingsBundle, SyncConst.SYNC_INTERVAL);

			//var intent = new Intent(Intent.ActionGetContent);
			//intent.SetType("*/*");
			//intent.AddCategory(Intent.CategoryOpenable);
			//StartActivityForResult(intent, PICKFILE_REQUEST_CODE);
			//string fileName = "bd0712a3-a5e7-4704-b565-889f673a393b.realm";
			//string dbFileLocation = Path.Combine(Helper.AppDir, fileName);

			//if (File.Exists(dbFileLocation)) {
			//	SDiag.Debug.WriteLine(dbFileLocation + " is Exists!");
			//}

			//if (File.Exists(MainDatabase.DBPath)) {
			//	SDiag.Debug.WriteLine(MainDatabase.DBPath + " is Exists!");
			//	//var fi = new FileInfo(MainDatabase.DBPath);
			//	//var directory = fi.Directory.FullName;
			//	var newPath = Path.Combine(new FileInfo(MainDatabase.DBPath).Directory.FullName, fileName);
			//	if (!File.Exists(newPath)) File.Copy(dbFileLocation, newPath, true);

			//	if (File.Exists(newPath)) {
			//		SDiag.Debug.WriteLine(newPath + " is Exists!");
			//		MainDatabase.Dispose();
			//		Helper.C_DB_FILE_NAME = fileName;
			//		MainDatabase.Username = Helper.Username;
			//	}
			//}
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (requestCode == PICKFILE_REQUEST_CODE) {
				// SDiag.Debug.WriteLine(resultCode);
				if (resultCode == Result.Ok) {
					StartActivity(new Intent(Intent.ActionView, data.Data));
				}
			}
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

			//int count = 0;
			foreach (var pharmacy in pharmacies) {
				//count++;
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
			FindViewById<TextView>(Resource.Id.tdaSaleDatas).Text = string.Format(@"SaleDatas: {0}", MainDatabase.GetItems<SaleDataByMonth>().Count);
			FindViewById<TextView>(Resource.Id.tdaPhotoDatas).Text = string.Format(@"PhotoDatas: {0}", MainDatabase.GetItems<PhotoData>().Count);

			var ll = FindViewById<LinearLayout>(Resource.Id.tdaDataLL);
			var distributors = MainDatabase.GetItems<Distributor>();
			foreach (var item in distributors) {
				ll.AddView(new TextView(this) { Text = string.Format("uuid:{0}, name:{1}", item.uuid, item.name) });
			}
		}

	}
}

