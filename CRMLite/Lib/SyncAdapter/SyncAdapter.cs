using System.Net;
using System.Collections.Generic;

using Android.OS;
using Android.App;
using Android.Util;
using Android.Content;
using Android.Accounts;
using Android.Support.V4.App;

using RestSharp;

using CRMLite.Dialogs;
using CRMLite.Entities;
using HockeyApp.Android;

namespace CRMLite.Lib.Sync
{
	public class SyncAdapter : AbstractThreadedSyncAdapter
	{
		const string TAG = "CRMLite.Lib.Sync:SyncAdapter";
		static readonly string[] EMPTY_STRING_ARRAY = new string[0];

		ContentResolver ContentResolver;
		readonly NotificationManager NotificationManager;

		int UPSERT;
		int DELETE;

		public SyncAdapter(Context context, bool autoInitialize) : base(context, autoInitialize)
		{
			ContentResolver = context.ContentResolver;
			NotificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
			CrashManager.Register(Context, Secret.HockeyappAppId, new MyCrashManagerListener { ContextHolder = new System.WeakReference<Context>(Context) });
		}

		// For Android 3.0 compat
		public SyncAdapter(Context context, bool autoInitialize, bool allowParallelSyncs)
			: base(context, autoInitialize, allowParallelSyncs)
		{
			ContentResolver = context.ContentResolver;
			NotificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
			CrashManager.Register(Context, Secret.HockeyappAppId, new MyCrashManagerListener { ContextHolder = new System.WeakReference<Context>(Context) });
		}

		public override void OnPerformSync(Account account, Bundle extras, string authority, ContentProviderClient provider, Android.Content.SyncResult syncResult)
		{
			// Data transfer code here
			var tag = TAG + ":OnPerformSync";

			var DB_PATH = extras.GetString(MainDatabase.C_DB_PATH, string.Empty);
			var LOC_PATH = extras.GetString(MainDatabase.C_LOC_PATH, string.Empty);
			var ACCESS_TOKEN = extras.GetString(SigninDialog.C_ACCESS_TOKEN, string.Empty);
			var HOST_URL = extras.GetString(SigninDialog.C_HOST_URL, string.Empty);
			//string HOST_URL = "http://sbl-crm-project-pafik13.c9users.io:8080/";

			bool hasBDPath = true;
			bool hasLocPath = true;
			bool hasAccessToken = true;
			bool hasHostURL = true;

			if (string.IsNullOrEmpty(DB_PATH)) {
				hasBDPath = false;
				Log.Error(tag, "DB_PATH is NULL");
			} else {
				Log.Info(tag, string.Concat("DB_PATH: ", DB_PATH));
			}

			if (string.IsNullOrEmpty(LOC_PATH)) {
				hasLocPath = false;
				Log.Error(tag, "LOC_PATH is NULL");
			} else {
				Log.Info(tag, string.Concat("LOC_PATH: ", LOC_PATH));
			}

			if (string.IsNullOrEmpty(ACCESS_TOKEN)) {
				hasAccessToken = false;
				Log.Error(tag, "ACCESS_TOKEN is NULL");
			} else {
				Log.Info(tag, string.Concat("ACCESS_TOKEN: ", ACCESS_TOKEN));
			}

			if (string.IsNullOrEmpty(HOST_URL)) {
				hasHostURL = false;
				Log.Error(tag, "HOST_URL is NULL");
			} else {
				Log.Info(tag, string.Concat("HOST_URL: ", HOST_URL));
			}
			
			if (hasBDPath && hasLocPath && hasAccessToken && hasHostURL) {
				var client = new RestClient(HOST_URL);

				// ПОЛУЧЕНИЕ ДАННЫХ
				var path = typeof(LifecycleAction).Name + "/byjwt";
				var req = new RestRequest(path, Method.GET);
				req.AddQueryParameter(@"access_token", ACCESS_TOKEN);

				var res = client.Execute<List<LifecycleAction>>(req);

				if ((res.StatusCode != HttpStatusCode.OK)
				  && (res.StatusCode != HttpStatusCode.Created)
				   ) {
					Log.Error(tag, string.Format("NOT Download LifecycleAction"));
				} else {
					// TODO: check for null res.Data (exception when at metro -- return 200 with html text)
					Log.Info(tag, string.Format("Download LifecycleAction: {0}", res.Data.Count));

					foreach (var lc_action in res.Data) {
						bool canClear = false;
						var modelURI = SyncConst.GetURI(lc_action.model);
						NotificationCompat.Builder ncbuilder;
						switch (lc_action.action) {
							case "create":
							case "update":
								var pathModel = string.Format("{0}/{1}?populate=false", lc_action.model, lc_action.uuid);
								var reqModel = new RestRequest(pathModel, Method.GET);
								reqModel.AddQueryParameter(@"access_token", ACCESS_TOKEN);

								var resModel = client.Execute(reqModel);
								switch (resModel.StatusCode) {
									case HttpStatusCode.OK:
									case HttpStatusCode.Created:
										Log.Info(tag, string.Format("Downloaded Model by path:{0}", pathModel));
										Log.Info(tag, string.Format("Downloaded Model={0}", resModel.Content));
										var values = new ContentValues();
										values.Put("db_path", DB_PATH);
										values.Put("model", lc_action.model);
										values.Put("uuid", lc_action.uuid);
										values.Put("action", lc_action.action);
										values.Put("json", resModel.Content);
										var result = provider.Insert(modelURI, values);
										switch (result.LastPathSegment) {
											case SyncConst._OK:
												canClear = true;
												ncbuilder = new NotificationCompat.Builder(Context)
												                                  .SetSmallIcon(Resource.Mipmap.Icon)
												                                  .SetContentTitle("Было добавление/обновление объекта")
												                                  .SetContentText(string.Format("object:{0}:{1}", lc_action.model, lc_action.uuid));
												NotificationManager.Notify("SBL-CRM", UPSERT++, ncbuilder.Build());
												break;
											case SyncConst._ERROR:
												Log.Error(tag, string.Format("NOT Inserted object:{0}:{1}", lc_action.model, lc_action.uuid));
												break;
											default:
												Log.Error(tag, string.Format("Unhandled LastPathSegment {0}", result.LastPathSegment));
												break;
										}
										break;
									default:
										Log.Error(tag, string.Format("NOT Downloaded Model by path:{0}", pathModel));
										break;
								}
								break;

							case "delete":
								var args = new string[] { DB_PATH, lc_action.uuid };
								int d = provider.Delete(modelURI, modelURI.LastPathSegment, args);
								if (d == -1) {
									Log.Error(tag, string.Format("NOT Deleted object:{0}:{1}", lc_action.model, lc_action.uuid));
									break;
								}
								canClear = true;
								ncbuilder = new NotificationCompat.Builder(Context)
								                                  .SetSmallIcon(Resource.Mipmap.Icon)
								                                  .SetContentTitle("Было удаление объекта")
								                                  .SetContentText(string.Format("object:{0}:{1}", lc_action.model, lc_action.uuid));
								NotificationManager.Notify("SBL-CRM", DELETE++, ncbuilder.Build());
								break;
							default:
								Log.Error(tag, string.Format("Unhandled action: {0}", lc_action.action));
								break;
						}

						if (canClear) {
							var pathClear = typeof(LifecycleAction).Name + "/clear";
							var reqClear = new RestRequest(pathClear, Method.DELETE);
							reqClear.AddQueryParameter(@"access_token", ACCESS_TOKEN);
							reqClear.AddQueryParameter(@"model", lc_action.model);
							reqClear.AddQueryParameter(@"uuid", lc_action.uuid);

							var resClear = client.Execute(reqClear);
							if ((resClear.StatusCode != HttpStatusCode.OK) && (resClear.StatusCode != HttpStatusCode.Created))
							{
								Log.Error(tag, string.Format("NOT Cleared object:{0}:{1}", lc_action.model, lc_action.uuid));
							}
						}
					}
				}

				// ОТДАЧА ДАННЫХ
				var entitiesArray = new string[] {       
					SyncConst.Attendancies,
					SyncConst.CompetitorDatas,
					SyncConst.ContractDatas,
					SyncConst.CoterieDatas,
					SyncConst.DistributionDatas,
		            SyncConst.DistributorDatas,
					SyncConst.Pharmacies,
					SyncConst.Employees,
					SyncConst.GPSDatas,
					SyncConst.Hospitals,
					SyncConst.HospitalDatas,
					SyncConst.Messages,
					SyncConst.MessageDatas,
					SyncConst.PresentationDatas,
					SyncConst.PromotionDatas,
					SyncConst.ResumeDatas,
					SyncConst.RouteItems,
					SyncConst.ExcludeRouteItems,
		            SyncConst.GPSLocations,
			        SyncConst.PhotoDatas
				};

				foreach (var entities in entitiesArray) {
					// 1. получить данные
					Log.Info(tag, string.Format("Start Query, {0}", entities));
					var entyitiesURI = SyncConst.GetURI(entities);
					var args = entities == SyncConst.GPSLocations ? new string[] { LOC_PATH } : new string[] { DB_PATH };
					var cursor = provider.Query(entyitiesURI, EMPTY_STRING_ARRAY, string.Empty, args, string.Empty);
					if (cursor == null) {
						Log.Info(tag, string.Format("End Query, cursor is NULL"));
					} else {
						Log.Info(tag, string.Format("End Query, {0} entities found", cursor.Count));
					}

					// 2. синхронизировать
					Log.Info(tag, string.Format("Start Sync, {0}", entities));
					var uuids = new List<string>();
					uuids.Add(entities == SyncConst.GPSLocations ? LOC_PATH : DB_PATH);
					if (cursor != null) {
						try {
							if (cursor.Count > 0) {
								cursor.MoveToFirst();
								do {
									var request = new RestRequest(cursor.GetString(0), Method.POST);
									//request.RequestFormat = DataFormat.Json;
									//request.AddBody(cursor.GetString(2));
									request.AddHeader("Accept", "application/json");
									request.Parameters.Clear();
									request.AddParameter("application/json", cursor.GetString(2), ParameterType.RequestBody);
									request.AddQueryParameter(@"access_token", ACCESS_TOKEN);


									var response = client.Execute(request);
									switch (response.StatusCode) {
										case HttpStatusCode.OK:
										case HttpStatusCode.Created:
											Log.Info(tag, string.Format("Uploaded: {0}-{1}", cursor.GetString(0), cursor.GetString(1)));
											uuids.Add(cursor.GetString(1));
											break;
										default:
											Log.Info(tag, string.Format("NOT Uploaded: {0}-{1}", cursor.GetString(0), cursor.GetString(1)));
											break;
									}
								} while (cursor.MoveToNext());
							}
						} catch (System.Exception ex) {
							Log.Error(tag, ex.Message);
						}
					}
					Log.Info(tag, "End Sync");

					// 3. обновить данные
					Log.Info(tag, string.Format("Start Update, {0}", entities));
					int count = 0;
					if (uuids.Count > 1) {
						try {
							var setSyncedURI = SyncConst.GetURI(SyncConst.SET_SYNCED);
							count = provider.Update(setSyncedURI, new ContentValues(), entyitiesURI.LastPathSegment, uuids.ToArray());
						} catch (System.Exception ex) {
							Log.Error(tag, ex.Message);
						}
					}
					Log.Info(tag, string.Format("End Update, {0} entities updated", count));
				}
			}
		}
	}
}

