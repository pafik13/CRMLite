using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

using SDiag = System.Diagnostics;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Content.PM;

using RestSharp;

using Realms;

using CRMLite.Dialogs;
using CRMLite.Entities;

namespace CRMLite
{
	[Activity(Label = "SyncActivity", ScreenOrientation = ScreenOrientation.Landscape)]
	public class SyncActivity : Activity
	{
		TextView Locker;
		string ACCESS_TOKEN;
		CancellationTokenSource CancelSource;
		CancellationToken CancelToken;
		public string HOST_URL { get; private set; }
		public string USERNAME { get; private set; }
		public string AGENT_UUID { get; private set; }

		public List<MaterialFile> MaterialFiles { get; private set; }

		//public List<Hospital> Hospitals { get; private set; }

		//public List<HospitalData> HospitalDatas { get; private set; }

		//public List<MessageData> MessageDatas { get; private set; }

		//public List<PresentationData> PresentationDatas { get; private set; }

		//public List<ResumeData> ResumeDatas { get; private set; }

		//public List<RouteItem> RouteItems { get; private set; }

		//public List<PromotionData> PromotionDatas { get; private set; }


		//public int Messages { get; private set; }
		public int Count { get; private set; }



		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Sync);

			using (var trans = MainDatabase.BeginTransaction()) {
				MainDatabase.DeleteItems<MaterialFile>();
				trans.Commit();
			}
			Locker = FindViewById<TextView>(Resource.Id.locker);
			
			FindViewById<Button>(Resource.Id.saCloseB).Click += (s, e) => {
				Finish();
			};
			
			FindViewById<Button>(Resource.Id.saSyncB).Click += Sync_Click;

			FindViewById<Button>(Resource.Id.saUploadPhotoB).Click += UploadPhoto_Click;


			var shared = GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private);

			ACCESS_TOKEN = shared.GetString(SigninDialog.C_ACCESS_TOKEN, string.Empty);
			USERNAME = shared.GetString(SigninDialog.C_USERNAME, string.Empty);
			HOST_URL = shared.GetString(SigninDialog.C_HOST_URL, string.Empty);
			AGENT_UUID = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);

			MaterialFiles = new List<MaterialFile>();


			RefreshView();
		}

		void UploadPhoto_Click(object sender, EventArgs e)
		{
			foreach (var photo in MainDatabase.GetItemsToSync<PhotoData>()) {
				try {
					Toast.MakeText(this, string.Format(@"Загрузка фото с uuid {0} по посещению с uuid:{1}", photo.UUID, photo.Attendance), ToastLength.Short).Show();

					var client = new RestClient(HOST_URL);

					var request = new RestRequest(@"PhotoData/upload", Method.POST);
					request.AddQueryParameter(@"access_token", ACCESS_TOKEN);
					//request.AddQueryParameter(@"Stamp", photo.Stamp.ToString());
					request.AddQueryParameter(@"Attendance", photo.Attendance);
					request.AddQueryParameter(@"PhotoType", photo.PhotoType);
					request.AddQueryParameter(@"Brand", photo.Brand);
					request.AddQueryParameter(@"Latitude", photo.Latitude.ToString(CultureInfo.CreateSpecificCulture(@"en-GB")));
					request.AddQueryParameter(@"Longitude", photo.Longitude.ToString(CultureInfo.CreateSpecificCulture(@"en-GB")));
					request.AddFile(@"photo", File.ReadAllBytes(photo.PhotoPath), Path.GetFileName(photo.PhotoPath), string.Empty);

					var response = client.Execute(request);

					switch (response.StatusCode) {
						case HttpStatusCode.OK:
						case HttpStatusCode.Created:
							photo.IsSynced = true;
							Toast.MakeText(this, @"Фото ЗАГРУЖЕНО!", ToastLength.Short).Show();
							continue;
						default:
							Toast.MakeText(this, @"Не удалось загрузить фото по посещению!", ToastLength.Short).Show();
							continue;
					}
				} catch (Exception ex) {
					Toast.MakeText(this, @"Error : " + ex.Message, ToastLength.Short).Show();
					continue;
				}
			}
		}

		void RefreshView(){
			//var pharmacies = MainDatabase.GetItemsToSync<Pharmacy>();

			Count = 0;

			Count += MainDatabase.CountItemsToSync<Attendance>();
			Count += MainDatabase.CountItemsToSync<CompetitorData>();
			Count += MainDatabase.CountItemsToSync<ContractData>();
			Count += MainDatabase.CountItemsToSync<CoterieData>();
			Count += MainDatabase.CountItemsToSync<DistributionData>();
			Count += MainDatabase.CountItemsToSync<Pharmacy>();
			Count += MainDatabase.CountItemsToSync<Employee>();

			Count += MainDatabase.CountItemsToSync<GPSData>();

			Count += MainDatabase.CountItemsToSync<Hospital>();
			Count += MainDatabase.CountItemsToSync<HospitalData>();

			//var monthFinanceDatas = MainDatabase.GetItemsToSync<FinanceDataByMonth>();
			//var quarterFinanceDatas = MainDatabase.GetItemsToSync<FinanceDataByQuarter>();
			//var monthSaleDatas = MainDatabase.GetItemsToSync<SaleDataByMonth>();
			//var quarterSaleDatas = MainDatabase.GetItemsToSync<SaleDataByQuarter>();

			Count += MainDatabase.CountItemsToSync<MessageData>();
			//var photoDatas = MainDatabase.GetItemsToSync<PhotoData>();

			Count += MainDatabase.CountItemsToSync<PresentationData>();
			Count += MainDatabase.CountItemsToSync<PromotionData>();
			Count += MainDatabase.CountItemsToSync<ResumeData>();
			Count += MainDatabase.CountItemsToSync<RouteItem>();


			Count += MainDatabase.CountItemsToSync<Entities.Message>();

			var toSyncCount = FindViewById<TextView>(Resource.Id.saSyncEntitiesCount);
			toSyncCount.Text = string.Format("Необходимо синхронизировать {0} объектов", Count);

			var toUpdateCount = FindViewById<TextView>(Resource.Id.saUpdateEntitiesCount);
			toUpdateCount.Text = string.Format("Необходимо обновить {0} объектов", 0);

			var photoCount = FindViewById<TextView>(Resource.Id.saSyncPhotosCount);
			photoCount.Text = string.Format("Необходимо выгрузить {0} фото", MainDatabase.CountItemsToSync<PhotoData>());


			CancelSource = new CancellationTokenSource();
			CancelToken = CancelSource.Token;
			CheckMaterialFiles();
		}

		async void CheckMaterialFiles()
		{
			var client = new RestClient(HOST_URL);
			//var client = new RestClient("http://sbl-crm-project-pafik13.c9users.io:8080/");
			var request = new RestRequest("/MaterialFile?populate=false", Method.GET);

			var response = await client.ExecuteGetTaskAsync<List<MaterialFile>>(request);

			if (!CancelToken.IsCancellationRequested) {
				switch (response.StatusCode) {
					case HttpStatusCode.OK:
					case HttpStatusCode.Created:
						SDiag.Debug.WriteLine("MaterialFile: {0}", response.Data.Count);
						MaterialFiles.Clear();
						foreach (var item in response.Data) {
							if (!MainDatabase.IsSavedBefore<MaterialFile>(item.uuid)) {
								if (!string.IsNullOrEmpty(item.s3Location)) {
									MaterialFiles.Add(item);
								}
							}
						}

						RunOnUiThread(() => {
							var toUpdateCount = FindViewById<TextView>(Resource.Id.saUpdateEntitiesCount);
							toUpdateCount.Text = string.Format("Необходимо обновить {0} объектов", MaterialFiles.Count);
						});
						break;
				}
				Console.WriteLine(response.StatusDescription);
			}
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

			if (Count > 0) {
				var progress = ProgressDialog.Show(this, string.Empty, @"Синхронизация");

				new Task(() => {
					MainDatabase.Dispose();
					Thread.Sleep(1000); // иначе не успеет показаться диалог

					MainDatabase.Username = USERNAME;
					//var types = new List<Type>();
					//types.Add(typeof(Attendance));
					//types.Add(typeof(CompetitorData));

					//foreach (var type in types) {
					//	SyncEntities(MainDatabase.GetItemsToSync<(type as Type)>());
					//}

					foreach (var materialFile in MaterialFiles) {
						if (!MainDatabase.IsSavedBefore<Material>(materialFile.material)) {
							var client = new RestClient(HOST_URL);
							string pathURL = string.Format(@"Material/{0}?populate=false", materialFile.material);
							var request = new RestRequest(pathURL, Method.GET);

							var response = client.Execute<Material>(request);

							switch (response.StatusCode) {
								case HttpStatusCode.OK:
								case HttpStatusCode.Created:
									SDiag.Debug.WriteLine("Material: {0}", response.Data);
									MainDatabase.SaveItem(response.Data);
									SDiag.Debug.WriteLine(response.StatusDescription);
									break;
								default:
									SDiag.Debug.WriteLine(response.StatusDescription);
									continue;
							}
						}

						var wclient = new WebClient();
						string path = new Java.IO.File(Helper.MaterialDir, materialFile.fileName).ToString(); 
						try {
							//ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
							wclient.DownloadFile(materialFile.s3Location, path);
							MainDatabase.SaveItem(materialFile);
						} catch(Exception exc) {
							SDiag.Debug.WriteLine("Was exception: {0}", exc.Message);
						}
					}

					SyncEntities(MainDatabase.GetItemsToSync<Attendance>());
					SyncEntities(MainDatabase.GetItemsToSync<CompetitorData>());
					SyncEntities(MainDatabase.GetItemsToSync<ContractData>());
					SyncEntities(MainDatabase.GetItemsToSync<CoterieData>());
					SyncEntities(MainDatabase.GetItemsToSync<DistributionData>());
					SyncEntities(MainDatabase.GetItemsToSync<Pharmacy>());
					SyncEntities(MainDatabase.GetItemsToSync<Employee>());
					SyncEntities(MainDatabase.GetItemsToSync<GPSData>());
					SyncEntities(MainDatabase.GetItemsToSync<Hospital>());
					SyncEntities(MainDatabase.GetItemsToSync<HospitalData>());
					SyncEntities(MainDatabase.GetItemsToSync<Entities.Message>());
					SyncEntities(MainDatabase.GetItemsToSync<MessageData>());
					SyncEntities(MainDatabase.GetItemsToSync<PresentationData>());
					SyncEntities(MainDatabase.GetItemsToSync<PromotionData>());
					SyncEntities(MainDatabase.GetItemsToSync<ResumeData>());
					SyncEntities(MainDatabase.GetItemsToSync<RouteItem>());

					RunOnUiThread(() => {
						MainDatabase.Dispose();
						MainDatabase.Username = USERNAME;
						// Thread.Sleep(1000);
						progress.Dismiss();
						RefreshView();
					});
				}).Start();
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
		}

		void SyncEntities<T>(List<T> items) where T : RealmObject, ISync
		{
			var client = new RestClient(HOST_URL); 
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

		protected override void OnPause()
		{
			base.OnPause();

			if (CancelToken.CanBeCanceled && CancelSource != null) {
				CancelSource.Cancel();
			}		
		}
	}
}

