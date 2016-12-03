using System;
using System.Net;
using Android.App;
using Android.Content;

using RestSharp;

namespace CRMLite.Services
{
	[Service]
	[IntentFilter(new string[] { "com.xamarin.SyncService" })]
	public class SyncService : IntentService
	{
		public SyncService() : base("SyncService")
		{
		}

		protected override void OnHandleIntent(Intent intent)
		{
			Console.WriteLine("perform some long running work");

			if (MainDatabase.GetQueueSize() > 0)
			{
				//var client = new RestClient(@"http://demo-project-pafik13.c9users.io:8080/");
				var client = new RestClient(@"http://front-sblcrm.rhcloud.com/");

				foreach (var item in MainDatabase.GetQueue())
				{
					if (!item.Value.IsSynced)
					{
						var request = new RestRequest(item.Value.Path, Method.POST);
						item.Value.TrySyncAt = DateTimeOffset.Now;
						request.AddParameter("body", item.Value.JSON, "application/json", ParameterType.RequestBody);
						//var response = client.Execute<Entities.SyncResult>(request);
						//request.AddJsonBody(item.Value);
						var response = client.Execute<Entities.SyncResult>(request);
						switch (response.StatusCode)
						{
							case HttpStatusCode.OK:
							case HttpStatusCode.Created:
								item.Value.IsSynced = true;
								//TODO: Rename to CacheData
								MainDatabase.SaveSyncResult(item.Value.ObjectUUID, response.Data);
								break;
						}
						Console.WriteLine(response.StatusDescription);
					}
				}
			}

			MainDatabase.ManageQueue();

			Console.WriteLine("work complete");
		}
	}
}
