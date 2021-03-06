﻿using System;
using System.IO;
using System.Net;
using System.Text;
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
using Android.Accounts;
using Android.Content.PM;

using RestSharp;

using Realms;

using Newtonsoft.Json;

using CRMLite.Dialogs;
using CRMLite.Entities;
using CRMLite.Lib.Sync;

namespace CRMLite
{
	[Activity(Label = "SyncActivity", ScreenOrientation = ScreenOrientation.Landscape)]
	public class SyncActivity : Activity
	{
		const string C_LAST_UPLOAD_REALM_FILE_DATETIME = "C_LAST_UPLOAD_REALM_FILE_DATETIME";

		TextView Locker;
		string ACCESS_TOKEN;
		CancellationTokenSource CancelSource;
		CancellationToken CancelToken;

		CancellationTokenSource CSForLibraryFiles;
		CancellationToken CTForLibraryFiles;

		public string HOST_URL { get; private set; }
		public string USERNAME { get; private set; }
		public string AGENT_UUID { get; private set; }

		public string LAST_UPLOAD_REALM_FILE_DATETIME { get; private set; }

		public List<WorkType> WorkTypes { get; private set; }

		public List<MaterialFile> MaterialFiles { get; private set; }
		public List<LibraryFile> LibraryFiles { get; private set; }

		public int Count { get; private set; }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Sync);

			//using (var trans = MainDatabase.BeginTransaction()) {
			//	MainDatabase.DeleteItems<MaterialFile>();
			//	trans.Commit();
			//}
			Locker = FindViewById<TextView>(Resource.Id.locker);
			
			FindViewById<Button>(Resource.Id.saCloseB).Click += (s, e) => {
				Finish();
			};
			
			FindViewById<Button>(Resource.Id.saSyncB).Click += Sync_Click;

			FindViewById<Button>(Resource.Id.saUploadPhotoB).Click += UploadPhoto_Click;

			FindViewById<Button>(Resource.Id.saUploadRealmB).Click += UploadRealm_Click;

			var shared = GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private);

			ACCESS_TOKEN = shared.GetString(SigninDialog.C_ACCESS_TOKEN, string.Empty);
			USERNAME = shared.GetString(SigninDialog.C_USERNAME, string.Empty);
			HOST_URL = shared.GetString(SigninDialog.C_HOST_URL, string.Empty);
			//HOST_URL = @"http://sbl-crm-project-pafik13.c9users.io:8080/";
			AGENT_UUID = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);

			MaterialFiles = new List<MaterialFile>();
			WorkTypes = new List<WorkType>();
			LibraryFiles = new List<LibraryFile>();

			RefreshView();
		}

		void UploadRealm_Click(object sender, EventArgs e)
		{
			var client = new RestClient(HOST_URL);

			var request = new RestRequest(@"RealmFile/upload", Method.POST);

			request.AddQueryParameter(@"access_token", ACCESS_TOKEN);
			request.AddQueryParameter(@"androidId", Helper.AndroidId);
			request.AddFile(@"realm", File.ReadAllBytes(MainDatabase.DBPath), Path.GetFileName(MainDatabase.DBPath), string.Empty);

			var response = client.Execute(request);

			switch (response.StatusCode) {
				case HttpStatusCode.OK:
				case HttpStatusCode.Created:
					SDiag.Debug.WriteLine("Удалось загрузить копию базы!");
					Toast.MakeText(this, "Удалось загрузить копию базы!", ToastLength.Short).Show();
					break;
				default:
					SDiag.Debug.WriteLine("Не удалось загрузить копию базы!");
					Toast.MakeText(this, "Не удалось загрузить копию базы!", ToastLength.Short).Show();
					break;
			}
		}

		void UploadPhoto_Click(object sender, EventArgs e)
		{
			if (CancelSource != null && CancelToken.CanBeCanceled) {
				CancelSource.Cancel();
			}
			if (CSForLibraryFiles != null && CTForLibraryFiles.CanBeCanceled) {
				CSForLibraryFiles.Cancel();
			}
			var progress = ProgressDialog.Show(this, string.Empty, @"Выгрузка фото");

			new Task(() => {
				MainDatabase.Dispose();
				Thread.Sleep(1000); // иначе не успеет показаться диалог

				MainDatabase.Username = USERNAME;

				var client = new RestClient(HOST_URL);
				IRestRequest request;
				IRestResponse response;
				//var client = new RestClient("http://sbl-crm-project-pafik13.c9users.io:8080/");

				bool isNeedUploadRealm = false;
				if (string.IsNullOrEmpty(LAST_UPLOAD_REALM_FILE_DATETIME)) {
					isNeedUploadRealm = true;
				} else {
					try {
						var date = DateTime.Parse(LAST_UPLOAD_REALM_FILE_DATETIME, null, DateTimeStyles.RoundtripKind);
						isNeedUploadRealm = (date.Date > DateTime.Now.Date);
					} catch (Exception exc) {
						SDiag.Debug.WriteLine("Error : " + exc.Message);
						isNeedUploadRealm = true;
					}
				}

				if (isNeedUploadRealm) {
					request = new RestRequest(@"RealmFile/upload", Method.POST);

					request.AddQueryParameter(@"access_token", ACCESS_TOKEN);
					request.AddQueryParameter(@"androidId", Helper.AndroidId);
					request.AddFile(@"realm", File.ReadAllBytes(MainDatabase.DBPath), Path.GetFileName(MainDatabase.DBPath), string.Empty);

					response = client.Execute(request);

					switch (response.StatusCode) {
						case HttpStatusCode.OK:
						case HttpStatusCode.Created:
							SDiag.Debug.WriteLine("Удалось загрузить копию базы!");
							string lastUploadRealmFileDatetime = DateTime.Now.ToString("O");
							GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private).Edit()
																									 .PutString(C_LAST_UPLOAD_REALM_FILE_DATETIME, lastUploadRealmFileDatetime)
																									 .Commit();
							break;
						default:
							SDiag.Debug.WriteLine("Не удалось загрузить копию базы!");
							break;
					}
					//RunOnUiThread(() => {
					//	MainDatabase.Dispose();
					//	MainDatabase.Username = USERNAME;
					//	// Thread.Sleep(1000);
					//	progress.Dismiss();
					//	RefreshView();
					//});
					//return;
				}
				using (var trans = MainDatabase.BeginTransaction()) {

					foreach (var photo in MainDatabase.GetItemsToSync<PhotoData>()) {
						try {
							//Toast.MakeText(this, string.Format(@"Загрузка фото с uuid {0} по посещению с uuid:{1}", photo.UUID, photo.Attendance), ToastLength.Short).Show();
							SDiag.Debug.WriteLine(string.Format(@"Загрузка фото с uuid {0} по посещению с uuid:{1}", photo.UUID, photo.Attendance));

							request = new RestRequest(@"PhotoData/upload", Method.POST);
							request.AddQueryParameter(@"access_token", ACCESS_TOKEN);
							//request.AddQueryParameter(@"Stamp", photo.Stamp.ToString());
							request.AddQueryParameter(@"Attendance", photo.Attendance);
							request.AddQueryParameter(@"PhotoType", photo.PhotoType);
							request.AddQueryParameter(@"Brand", photo.Brand);
							request.AddQueryParameter(@"Latitude", photo.Latitude.ToString(CultureInfo.CreateSpecificCulture(@"en-GB")));
							request.AddQueryParameter(@"Longitude", photo.Longitude.ToString(CultureInfo.CreateSpecificCulture(@"en-GB")));
							request.AddFile(@"photo", File.ReadAllBytes(photo.PhotoPath), Path.GetFileName(photo.PhotoPath), string.Empty);

							response = client.Execute(request);

							switch (response.StatusCode) {
								case HttpStatusCode.OK:
								case HttpStatusCode.Created:
									// TODO: переделать на вызов с проверкой открытой транзакции
									photo.IsSynced = true;
									if (!photo.IsManaged) MainDatabase.SavePhoto(photo);
									//Toast.MakeText(this, "Фото ЗАГРУЖЕНО!", ToastLength.Short).Show();
									SDiag.Debug.WriteLine("Фото ЗАГРУЖЕНО!");
									continue;
								default:
									//Toast.MakeText(this, "Не удалось загрузить фото по посещению!", ToastLength.Short).Show();
									SDiag.Debug.WriteLine("Не удалось загрузить фото по посещению!");
									continue;
							}
						} catch (Exception ex) {
							//Toast.MakeText(this, @"Error : " + ex.Message, ToastLength.Short).Show();
							SDiag.Debug.WriteLine("Error : " + ex.Message);
							continue;
						}
					}
					trans.Commit();
				}

				MainDatabase.Dispose();
				RunOnUiThread(() => {
					MainDatabase.Username = USERNAME;
					// Thread.Sleep(1000);
					progress.Dismiss();
					RefreshView();
				});
			}).Start();
		}

		void RefreshView(){
			//var pharmacies = MainDatabase.GetItemsToSync<Pharmacy>();
			LAST_UPLOAD_REALM_FILE_DATETIME = GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private).GetString(C_LAST_UPLOAD_REALM_FILE_DATETIME, string.Empty);

			Count = 0;

			Count += MainDatabase.CountItemsToSync<Attendance>();
			Count += MainDatabase.CountItemsToSync<CompetitorData>();
			Count += MainDatabase.CountItemsToSync<ContractData>();
			Count += MainDatabase.CountItemsToSync<CoterieData>();
			Count += MainDatabase.CountItemsToSync<DistributionData>();
			Count += MainDatabase.CountItemsToSync<DistributorData>();
			Count += MainDatabase.CountItemsToSync<Pharmacy>();
			Count += MainDatabase.CountItemsToSync<Employee>();
			Count += MainDatabase.CountItemsToSync<ExcludeRouteItem>();

			Count += MainDatabase.CountItemsToSync<GPSData>();

			Count += MainDatabase.CountItemsToSync<Hospital>();
			Count += MainDatabase.CountItemsToSync<HospitalData>();

			Count += MainDatabase.CountItemsToSync<Entities.Message>();			
			Count += MainDatabase.CountItemsToSync<MessageData>();
			//var photoDatas = MainDatabase.GetItemsToSync<PhotoData>();

			Count += MainDatabase.CountItemsToSync<PresentationData>();
			Count += MainDatabase.CountItemsToSync<PromotionData>();
			Count += MainDatabase.CountItemsToSync<ResumeData>();
			Count += MainDatabase.CountItemsToSync<RouteItem>();


			var toSyncCount = FindViewById<TextView>(Resource.Id.saSyncEntitiesCount);
			toSyncCount.Text = string.Format("Необходимо синхронизировать {0} объектов", Count);

			var toUpdateCount = FindViewById<TextView>(Resource.Id.saUpdateEntitiesCount);
			toUpdateCount.Text = string.Format("Необходимо обновить {0} объектов", 0);

			var photoCount = FindViewById<TextView>(Resource.Id.saSyncPhotosCount);
			photoCount.Text = string.Format("Необходимо выгрузить {0} фото", MainDatabase.CountItemsToSync<PhotoData>());


			CancelSource = new CancellationTokenSource();
			CancelToken = CancelSource.Token;
			CheckMaterialFiles();

			CSForLibraryFiles = new CancellationTokenSource();
			CTForLibraryFiles = CSForLibraryFiles.Token;
			CheckLibraryFiles();
		}

		async void CheckMaterialFiles()
		{
			var client = new RestClient(HOST_URL);
			//var client = new RestClient("http://sbl-crm-project-pafik13.c9users.io:8080/");
			var request = new RestRequest("/MaterialFile?type=for_pharmacy&populate=false", Method.GET);

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
							int count = MaterialFiles.Count + WorkTypes.Count + LibraryFiles.Count;
							FindViewById<TextView>(Resource.Id.saUpdateEntitiesCount).Text = string.Format("Необходимо обновить {0} объектов", count);
						});
						break;
				}
				SDiag.Debug.WriteLine(response.StatusDescription);
			}

			request = new RestRequest("/WorkType?populate=false", Method.GET);

			var responseWTs = await client.ExecuteGetTaskAsync<List<WorkType>>(request);

			if (!CancelToken.IsCancellationRequested) {
				switch (responseWTs.StatusCode) {
					case HttpStatusCode.OK:
					case HttpStatusCode.Created:
						SDiag.Debug.WriteLine("WorkType: {0}", responseWTs.Data.Count);
						WorkTypes.Clear();
						foreach (var item in responseWTs.Data) {
							if (!MainDatabase.IsSavedBefore<WorkType>(item.uuid)) {
								if (!string.IsNullOrEmpty(item.name)) {
									WorkTypes.Add(item);
								}
							}
						}

						RunOnUiThread(() => {
							int count = MaterialFiles.Count + WorkTypes.Count + LibraryFiles.Count;
							FindViewById<TextView>(Resource.Id.saUpdateEntitiesCount).Text = string.Format("Необходимо обновить {0} объектов", count);
						});
						break;
				}
				SDiag.Debug.WriteLine(response.StatusDescription);
			}
		}

		async void CheckLibraryFiles()
		{
			var client = new RestClient(HOST_URL);
			//var client = new RestClient("http://sbl-crm-project-pafik13.c9users.io:8080/");
			var request = new RestRequest("/LibraryFile?type=for_pharmacy&populate=false", Method.GET);

			var response = await client.ExecuteGetTaskAsync<List<LibraryFile>>(request);

			if (!CTForLibraryFiles.IsCancellationRequested) {
				switch (response.StatusCode) {
					case HttpStatusCode.OK:
					case HttpStatusCode.Created:
						SDiag.Debug.WriteLine("MaterialFile: {0}", response.Data.Count);
						LibraryFiles.Clear();
						foreach (var item in response.Data) {
							if (!MainDatabase.IsSavedBefore<LibraryFile>(item.uuid)) {
								if (!string.IsNullOrEmpty(item.s3Location)) {
									LibraryFiles.Add(item);
								}
							}
						}

						RunOnUiThread(() => {
							int count = MaterialFiles.Count + WorkTypes.Count + LibraryFiles.Count;
							FindViewById<TextView>(Resource.Id.saUpdateEntitiesCount).Text = string.Format("Необходимо обновить {0} объектов", count);
						});
						break;
				}
				SDiag.Debug.WriteLine(response.StatusDescription);
			}
		}

		public bool IsTokenExpired(string token)
		{
			var payloadBytes = Convert.FromBase64String(token.Split('.')[1] + "=");
			var payloadStr = Encoding.UTF8.GetString(payloadBytes, 0, payloadBytes.Length);

			// Here, I only extract the "exp" payload property. You can extract other properties if you want.
			var payload = JsonConvert.DeserializeAnonymousType(payloadStr, new { Exp = 0UL }); // 0UL makes implicit typing create the field as unsigned long. 

			var currentTimestamp = (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;

			return currentTimestamp > payload.Exp;
		}

		public Account CreateSyncAccount(Context context)
		{
			var newAccount = new Account(SyncConst.ACCOUNT, SyncConst.ACCOUNT_TYPE);

			var accountManager = (AccountManager)context.GetSystemService(AccountService);

			var tag = "CRMLite:SyncActivity:CreateSyncAccount";
			if (accountManager.AddAccountExplicitly(newAccount, null, null)) {
				Android.Util.Log.Info(tag, "AddAccountExplicitly");
			} else {
				Android.Util.Log.Info(tag, "NOT AddAccountExplicitly");
			}

			return newAccount;
		}

		void Sync_Click(object sender, EventArgs e){

			if (CancelSource != null && CancelToken.CanBeCanceled) {
				CancelSource.Cancel();
			}

			if (IsTokenExpired(ACCESS_TOKEN)) {

				var input = new EditText(this);
				// Specify the type of input expected; this, for example, sets the input as a password, and will mask the text
				input.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;

				new AlertDialog.Builder(this)
							   .SetTitle("Ключ доступа просрочен. Введите пароль.")
							   .SetView(input)
				               .SetPositiveButton("OK", async (caller, arguments) => {
								   var progress = ProgressDialog.Show(this, string.Empty, @"Проверка данных. Получение ключа доступа.");
								   var login = new RestClient(@"http://front-sblcrm.rhcloud.com/");
								   //var client = new RestClient(@"http://sbl-crm-project-pafik13.c9users.io:8080/");
								   login.CookieContainer = new CookieContainer();

								   string email = Helper.Username + "@sbl-crm.ru";
								   string password = input.Text;
								   IRestResponse response;
								   try {
									   var request = new RestRequest(@"auth/login", Method.POST);
									   request.AddParameter("email", email, ParameterType.GetOrPost);
									   request.AddParameter("password", password, ParameterType.GetOrPost);
									   response = await login.ExecuteTaskAsync(request);
									   if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created) {

										   request = new RestRequest(@"user/jwt", Method.GET);
										   response = await login.ExecuteTaskAsync<JsonWebToken>(request);
										   if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created) {

											   ACCESS_TOKEN = (response as IRestResponse<JsonWebToken>).Data.token;
											   GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private).Edit()
																													    .PutString(SigninDialog.C_ACCESS_TOKEN, ACCESS_TOKEN)
																													    .Commit();
												Toast.MakeText(this, "Новый ключ получен. Попробуйте синхронизироваться!", ToastLength.Long).Show();
											   progress.Dismiss();
										   } else {

											   progress.Dismiss();
										   }
									   } else {
										   progress.Dismiss();
									   }
								   } catch (Exception ex) {
									   Toast.MakeText(this, string.Format(@"Error: {0}", ex.Message), ToastLength.Long).Show();
									   progress.Dismiss();
								   }
							   })
				               .SetNegativeButton(Resource.String.cancel_button, (caller, arguments) => {
								   if (caller is Dialog) {
									   ((Dialog)caller).Dismiss();
								   }
							   })
							   .Show();

				return;
			}
			// Register Account and Set periodic sync
			var account = CreateSyncAccount(this);

			var settingsBundle = new Bundle();
			settingsBundle.PutString(MainDatabase.C_DB_PATH, MainDatabase.DBPath);
			settingsBundle.PutString(SigninDialog.C_ACCESS_TOKEN, ACCESS_TOKEN);
			settingsBundle.PutString(SigninDialog.C_HOST_URL, HOST_URL);
			settingsBundle.PutBoolean(ContentResolver.SyncExtrasExpedited, false);
			settingsBundle.PutBoolean(ContentResolver.SyncExtrasDoNotRetry, false);
			settingsBundle.PutBoolean(ContentResolver.SyncExtrasManual, false);

			ContentResolver.SetIsSyncable(account, SyncConst.AUTHORITY, 1);
			ContentResolver.SetSyncAutomatically(account, SyncConst.AUTHORITY, true);

			ContentResolver.AddPeriodicSync(account, SyncConst.AUTHORITY, settingsBundle, SyncConst.SYNC_INTERVAL);

			// End - Register Account and Set periodic sync


			try {
				var dbFileInfo = new FileInfo(MainDatabase.DBPath);
				HockeyApp.MetricsManager.TrackEvent(
					"SyncActivity.Sync_Click.DB",
					new Dictionary<string, string> { 
						{ "realm.path", dbFileInfo.FullName },
						{ "android_id", Helper.AndroidId },
						{ "agent_uuid", MainDatabase.AgentUUID }
					},
					new Dictionary<string, double> { { "realm.size", dbFileInfo.Length } }
				);
			} catch (Exception exc) {
				HockeyApp.MetricsManager.TrackEvent(
					"SyncActivity.Sync_Click.Exception",
					new Dictionary<string, string> { 
						{ "Message", exc.Message },
						{ "android_id", Helper.AndroidId },
						{ "agent_uuid", MainDatabase.AgentUUID }
					},
					new Dictionary<string, double> { { "HResult", exc.HResult } }
				);				
			}


			if (Count > 0 || MaterialFiles.Count > 0 || WorkTypes.Count > 0) {
				var progress = ProgressDialog.Show(this, string.Empty, @"Синхронизация");

				new Task(() => {
					MainDatabase.Dispose();
					Thread.Sleep(1000); // иначе не успеет показаться диалог

					MainDatabase.Username = USERNAME;

					// Обновление материалов
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

						// 79 Characters (72 without spaces)
						ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
						using (var wclient = new WebClient()) {
							string path = new Java.IO.File(Helper.MaterialDir, materialFile.fileName).ToString();
							try {
								wclient.DownloadFile(materialFile.s3Location, path);
								MainDatabase.SaveItem(materialFile);
							} catch (Exception exc) {
								// TODO: Add log info to HockeyApp
								SDiag.Debug.WriteLine("Was exception: {0}", exc.Message);
							}
						}
					}

					// Обновление материалов
					foreach (var workType in WorkTypes) {
						if (!MainDatabase.IsSavedBefore<WorkType>(workType.uuid)) {
							MainDatabase.SaveItem(workType);
						}
					}

					// Обновление файлов в библотеке
					foreach (var libraryFile in LibraryFiles) {
						// 79 Characters (72 without spaces)
						ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
						using (var wclient = new WebClient()) {
							string path = new Java.IO.File(Helper.LibraryDir, libraryFile.fileName).ToString();
							try {
								wclient.DownloadFile(libraryFile.s3Location, path);
								MainDatabase.SaveItem(libraryFile);
							} catch (Exception exc) {
								// TODO: Add log info to HockeyApp
								SDiag.Debug.WriteLine("Was exception: {0}", exc.Message);
							}
						}
					}

					SyncEntities(MainDatabase.GetItemsToSync<Attendance>());
					SyncEntities(MainDatabase.GetItemsToSync<CompetitorData>());
					SyncEntities(MainDatabase.GetItemsToSync<ContractData>());
					SyncEntities(MainDatabase.GetItemsToSync<CoterieData>());
					SyncEntities(MainDatabase.GetItemsToSync<DistributionData>());
					SyncEntities(MainDatabase.GetItemsToSync<DistributorData>());
					SyncEntities(MainDatabase.GetItemsToSync<Employee>());
					SyncEntities(MainDatabase.GetItemsToSync<ExcludeRouteItem>());
					SyncEntities(MainDatabase.GetItemsToSync<GPSData>());
					SyncEntities(MainDatabase.GetItemsToSync<Hospital>());
					SyncEntities(MainDatabase.GetItemsToSync<HospitalData>());
					SyncEntities(MainDatabase.GetItemsToSync<Entities.Message>());
					SyncEntities(MainDatabase.GetItemsToSync<MessageData>());
					SyncEntities(MainDatabase.GetItemsToSync<Pharmacy>());
					SyncEntities(MainDatabase.GetItemsToSync<PresentationData>());
					SyncEntities(MainDatabase.GetItemsToSync<PromotionData>());
					SyncEntities(MainDatabase.GetItemsToSync<ResumeData>());
					SyncEntities(MainDatabase.GetItemsToSync<RouteItem>());

					MainDatabase.Dispose();

					RunOnUiThread(() => {
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

			Helper.CheckIfTimeChangedAndShowDialog(this);
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
					SDiag.Debug.WriteLine(response.StatusDescription);
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

