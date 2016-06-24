using System;
using System.Threading;
using Android.App;
using Android.Content;

using Realms;

using RestSharp;

using CRMLite.Entities;

namespace CRMLite.Services
{
	[Service]
	[IntentFilter(new String[] { "com.xamarin.SyncService" })]
	public class SyncService : IntentService
	{
		public SyncService() : base("SyncService")
		{
		}

		protected override void OnHandleIntent(Intent intent)
		{
			Console.WriteLine("perform some long running work");

			using (var realm = Realm.GetInstance())
			{
				foreach (var item in realm.All<Pharmacy>())
				{
					Console.WriteLine(item.UUID);
					RestClient client = new RestClient(@"http://demo-project-pafik13.c9users.io:8080/");
					var request = new RestRequest(@"Pharmacy", Method.POST);
					request.AddJsonBody(item);
					var response = client.Execute(request);
					Console.WriteLine(response.StatusDescription);
				}
			}

			Console.WriteLine("work complete");
		}
	}
}
