using System;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Widget;

using RestSharp;

using CRMLite.Entities;
using Android.Views;
using System.Threading;
using Realms;
using System.Net;
using Android.Content;

using CRMLite.Dialogs;
using System.Threading.Tasks;

namespace CRMLite
{
	[Activity(Label = "SyncActivity")]
	public class SyncActivity : Activity
	{
		TextView Locker;
		string ACCESS_TOKEN;

		List<Attendance> Attendancies;
		List<CompetitorData> CompetitorDatas;
		List<ContractData> ContractDatas;
		List<CoterieData> CoterieDatas;
		List<DistributionData> DistributionDatas;

		public List<Hospital> Hospitals { get; private set; }

		public List<HospitalData> HospitalDatas { get; private set; }

		public List<MessageData> MessageDatas { get; private set; }

		public List<PresentationData> PresentationDatas { get; private set; }

		public List<ResumeData> ResumeDatas { get; private set; }

		public List<RouteItem> RouteItems { get; private set; }

		public List<PromotionData> PromotionDatas { get; private set; }


		List<Employee> Employees;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Sync);
			
			Locker = FindViewById<TextView>(Resource.Id.locker);
			
			FindViewById<Button>(Resource.Id.saCloseB).Click += (s, e) => {
				Finish();
			};
			
			FindViewById<Button>(Resource.Id.saSyncB).Click += Sync_Click;
			
			RefreshView();

			ACCESS_TOKEN = GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private)
				.GetString(SigninDialog.C_ACCESS_TOKEN, string.Empty);
		}
		
		void RefreshView(){
			//var pharmacies = MainDatabase.GetItemsToSync<Pharmacy>();


			Attendancies = MainDatabase.GetItemsToSync<Attendance>();
			CompetitorDatas = MainDatabase.GetItemsToSync<CompetitorData>();
			ContractDatas = MainDatabase.GetItemsToSync<ContractData>();
			CoterieDatas = MainDatabase.GetItemsToSync<CoterieData>();
			DistributionDatas = MainDatabase.GetItemsToSync<DistributionData>();
			Employees = MainDatabase.GetItemsToSync<Employee>();

			Hospitals = MainDatabase.GetItemsToSync<Hospital>();
			HospitalDatas = MainDatabase.GetItemsToSync<HospitalData>();

			//var monthFinanceDatas = MainDatabase.GetItemsToSync<FinanceDataByMonth>();
			//var quarterFinanceDatas = MainDatabase.GetItemsToSync<FinanceDataByQuarter>();
			//var monthSaleDatas = MainDatabase.GetItemsToSync<SaleDataByMonth>();
			//var quarterSaleDatas = MainDatabase.GetItemsToSync<SaleDataByQuarter>();

			MessageDatas = MainDatabase.GetItemsToSync<MessageData>();
			//var photoDatas = MainDatabase.GetItemsToSync<PhotoData>();

			PresentationDatas = MainDatabase.GetItemsToSync<PresentationData>();
			PromotionDatas = MainDatabase.GetItemsToSync<PromotionData>();
			ResumeDatas = MainDatabase.GetItemsToSync<ResumeData>();
			RouteItems = MainDatabase.GetItemsToSync<RouteItem>();

			FindViewById<TextView>(Resource.Id.saSyncEntitiesCount).Text = 
				( Attendancies.Count + + CompetitorDatas.Count + ContractDatas.Count + CoterieDatas.Count 
				 + DistributionDatas.Count + Employees.Count + Hospitals.Count + HospitalDatas.Count
				 + MessageDatas.Count + PresentationDatas.Count + PromotionDatas.Count + ResumeDatas.Count 
				 + RouteItems.Count
				).ToString();
				//pharmacies.Count + employees.Count + hospitals.Count + hospitalDatas.Count + attendances.Count + competitorDatas.Count +
				//contractDatas.Count + coterieDatas.Count + monthFinanceDatas.Count + quarterFinanceDatas.Count + monthSaleDatas.Count +
				//quarterSaleDatas.Count + messageDatas.Count + photoDatas.Count + presentationDatas.Count + promotionDatas.Count + 
				//resumeDatas.Count + routeItems.Count;
		}
		
		void Sync_Click(object sender, EventArgs e){
			Toast.MakeText(this, @"saSyncB_Click", ToastLength.Short).Show();

			//Locker.Visibility = ViewStates.Visible;

			//Thread.Sleep(2000);

			//var pharmacies = MainDatabase.GetItemsToSync<Pharmacy>();
			//var employees = MainDatabase.GetItemsToSync<Employee>();
			//var hospitals = MainDatabase.GetItemsToSync<Hospital>();
			//var hospitalDatas = MainDatabase.GetItemsToSync<HospitalData>();
			//var attendances = MainDatabase.GetItemsToSync<Attendance>();
			//var competitorDatas = MainDatabase.GetItemsToSync<CompetitorData>();
			//var contractDatas = MainDatabase.GetItemsToSync<ContractData>();
			//var coterieDatas = MainDatabase.GetItemsToSync<CoterieData>();

			//var monthFinanceDatas = MainDatabase.GetItemsToSync<FinanceDataByMonth>();
			//var quarterFinanceDatas = MainDatabase.GetItemsToSync<FinanceDataByQuarter>();
			//var monthSaleDatas = MainDatabase.GetItemsToSync<SaleDataByMonth>();
			//var quarterSaleDatas = MainDatabase.GetItemsToSync<SaleDataByQuarter>();

			//var messageDatas = MainDatabase.GetItemsToSync<MessageData>();
			//var photoDatas = MainDatabase.GetItemsToSync<PhotoData>();

			//var presentationDatas = MainDatabase.GetItemsToSync<PresentationData>();
			//var promotionDatas = MainDatabase.GetItemsToSync<PromotionData>();
			//var resumeDatas = MainDatabase.GetItemsToSync<ResumeData>();
			//var routeItems = MainDatabase.GetItemsToSync<RouteItem>();

			//Locker.Visibility = ViewStates.Gone;

			var progress = ProgressDialog.Show(this, string.Empty, @"Синхронизация");

			new Task(() => {
				Thread.Sleep(2000); // иначе не успеет показаться диалог

				RunOnUiThread(() => {
					SyncEntities(Attendancies);
					SyncEntities(CompetitorDatas);
					SyncEntities(ContractDatas);
					SyncEntities(CoterieDatas);
					SyncEntities(DistributionDatas);
					SyncEntities(Employees);
					SyncEntities(Hospitals);
					SyncEntities(HospitalDatas);
					SyncEntities(MessageDatas);
					SyncEntities(PresentationDatas);
					SyncEntities(PromotionDatas);
					SyncEntities(ResumeDatas);
					SyncEntities(RouteItems);

					progress.Dismiss();
					RefreshView();
				});
			}).Start();
		}

		protected override void OnResume()
		{
			base.OnResume();

		}
		
		void SyncItems(List<ISync> items)
		{
			var firstItem = items[0];
			string itemPath = firstItem.GetType().Name;
			//var client = new RestClient(@"http://front-sblcrm.rhcloud.com/");
			var client = new RestClient(@"http://sbl-crm-project-pafik13.c9users.io:8080/");

			foreach (var item in items)
			{
				var request = new RestRequest(itemPath, Method.POST);
				request.AddJsonBody(item);
				var response = client.Execute<Entities.SyncResult>(request);
				switch (response.StatusCode)
				{
					case HttpStatusCode.OK:
					case HttpStatusCode.Created:
						using (var trans = MainDatabase.BeginTransaction()) {
							item.SyncResult = response.Data;
							trans.Commit();
						}
						break;
				}
				Console.WriteLine(response.StatusDescription);
			}
		}

		void SyncEntities<T>(List<T> items) where T : RealmObject, ISync
		{
			//var client = new RestClient(@"http://front-sblcrm.rhcloud.com/");
			var client = new RestClient(@"http://sbl-crm-project-pafik13.c9users.io:8080/");
			string entityPath = typeof(T).Name;

			using (var trans = MainDatabase.BeginTransaction()) {
				foreach (var item in items) {
					var request = new RestRequest(entityPath, Method.POST);
					request.AddQueryParameter(@"access_token", ACCESS_TOKEN);
					request.JsonSerializer = new NewtonsoftJsonSerializer();
					request.AddJsonBody(item);
					var response = client.Execute(request);
					switch (response.StatusCode) {
						case HttpStatusCode.OK:
						case HttpStatusCode.Created:
							item.IsSynced = true;
							break;
					}
					Console.WriteLine(response.StatusDescription);
				}
				trans.Commit();
			}
		}
	}
}

