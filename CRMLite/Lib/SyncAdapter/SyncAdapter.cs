﻿using System.Net;
using System.Collections.Generic;

using Android.OS;
using Android.Util;
using Android.Content;
using Android.Accounts;

using RestSharp;

using CRMLite.Dialogs;
using CRMLite.Entities;

namespace CRMLite.Lib.Sync
{
	public class SyncAdapter : AbstractThreadedSyncAdapter
	{
		const string TAG = "CRMLite.Lib.Sync:SyncAdapter";
		static readonly string[] EMPTY_STRING_ARRAY = new string[0];

		ContentResolver ContentResolver;

		public SyncAdapter(Context context, bool autoInitialize) : base(context, autoInitialize)
		{
			ContentResolver = context.ContentResolver;
		}

		// For Android 3.0 compat
		public SyncAdapter(Context context, bool autoInitialize, bool allowParallelSyncs)
			: base(context, autoInitialize, allowParallelSyncs)
		{
			ContentResolver = context.ContentResolver;
		}

		public override void OnPerformSync(Account account, Bundle extras, string authority, ContentProviderClient provider, Android.Content.SyncResult syncResult)
		{
			// Data transfer code here
			var tag = TAG + ":OnPerformSync";

			var DB_PATH = extras.GetString(MainDatabase.C_DB_PATH, string.Empty);
			var ACCESS_TOKEN = extras.GetString(SigninDialog.C_ACCESS_TOKEN, string.Empty);
			var HOST_URL = extras.GetString(SigninDialog.C_HOST_URL, string.Empty);

			bool hasBDPath = true;
			bool hasAccessToken = true;
			bool hasHostURL = true;

			if (string.IsNullOrEmpty(DB_PATH)) {
				hasBDPath = false;
				Log.Error(tag, "DB_PATH is NULL");
			}

			if (string.IsNullOrEmpty(ACCESS_TOKEN)) {
				hasAccessToken = false;
				Log.Error(tag, "ACCESS_TOKEN is NULL");
			}

			if (string.IsNullOrEmpty(HOST_URL)) {
				hasHostURL = false;
				Log.Error(tag, "HOST_URL is NULL");
			}

			if (hasBDPath && hasAccessToken && hasHostURL) {
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
					Log.Info(tag, string.Format("Download LifecycleAction: {0}", res.Data.Count));

					foreach (var lc_action in res.Data) {
						bool canClear = false;
						var modelURI = SyncConst.GetURI(lc_action.model);

						switch (lc_action.action) {
							case "create":
							case "update":
								var pathModel = string.Format("{0}/{1}", lc_action.model, lc_action.uuid);
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
											case "OK":
												canClear = true;
												break;
											case "ERROR":
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
				};

				foreach (var entities in entitiesArray) {
					// 1. получить данные
					Log.Info(tag, string.Format("Start Query, {0}", entities));
					var entyitiesURI = SyncConst.GetURI(entities);
					var args = new string[] { DB_PATH };
					var cursor = provider.Query(entyitiesURI, EMPTY_STRING_ARRAY, string.Empty, args, string.Empty);
					if (cursor == null) {
						Log.Info(tag, string.Format("End Query, cursor is NULL"));
					} else {
						Log.Info(tag, string.Format("End Query, {0} entities found", cursor.Count));
					}

					// 2. синхронизировать
					Log.Info(tag, string.Format("Start Sync, {0}", entities));
					var uuids = new List<string>();
					uuids.Add(DB_PATH);
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
					try{
						var setSyncedURI = SyncConst.GetURI(SyncConst.SET_SYNCED);
						count = provider.Update(setSyncedURI, new ContentValues(), entyitiesURI.LastPathSegment, uuids.ToArray());
					} catch (System.Exception ex) {
						Log.Error(tag, ex.Message);
					}
					Log.Info(tag, string.Format("End Update, {0} entities updated", count));
				}
			}
		}
	}
}

