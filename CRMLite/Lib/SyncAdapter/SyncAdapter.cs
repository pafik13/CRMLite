using SD = System.Diagnostics;

using Android.OS;
using Android.Content;
using Android.Accounts;
using Android.Util;
using RestSharp;
using CRMLite.Dialogs;
using System.Net;
using System.Collections.Generic;

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

		public override void OnPerformSync(Account account, Bundle extras, string authority, ContentProviderClient provider, SyncResult syncResult)
		{
			// Data transfer code here
			var tag = TAG + ":OnPerformSync";

			var ACCESS_TOKEN = extras.GetString(SigninDialog.C_ACCESS_TOKEN, string.Empty);
			var HOST_URL = extras.GetString(SigninDialog.C_HOST_URL, string.Empty);

			bool hasAccessToken = true;
			bool hasHostURL = true;

			if (string.IsNullOrEmpty(ACCESS_TOKEN)) {
				hasAccessToken = false;
				Log.Error(tag, "ACCESS_TOKEN is NULL");
			}

			if (string.IsNullOrEmpty(HOST_URL)) {
				hasHostURL = false;
				Log.Error(tag, "HOST_URL is NULL");
			}

			if (hasAccessToken && hasHostURL) {
					
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
					var cursor = provider.Query(entyitiesURI, EMPTY_STRING_ARRAY, string.Empty, EMPTY_STRING_ARRAY, string.Empty);
					if (cursor == null) {
						Log.Info(tag, string.Format("End Query, cursor is NULL"));
					} else {
						Log.Info(tag, string.Format("End Query, {0} entities found", cursor.Count));
					}

					// 2. синхронизировать
					Log.Info(tag, string.Format("Start Sync, {0}", entities));
					var uuids = new List<string>();
					if (cursor != null) {
						try {
							var client = new RestClient(HOST_URL);

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


							//using (var trans = MainDatabase.BeginTransaction()) {
							//	foreach (var item in items) {
							//		request.JsonSerializer = new NewtonsoftJsonSerializer();

							//		SDiag.Debug.WriteLine(response.StatusDescription);
							//	}
							//	trans.Commit();
							//}

							//Log.Info(tag, string.Join(", ", cursor.GetColumnNames()));

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

