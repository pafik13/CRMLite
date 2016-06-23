using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Content;

using Realms;

using CRMLite.Entities;
using CRMLite.Adapters;

namespace CRMLite
{
	[Activity(Label = "Main")]
	public class MainActivity : Activity
	{

		List<Pharmacy> pharmacies = new List<Pharmacy>();
		ListView listView;
		PharmacyAdapter adapter;
		Realm realm;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			listView = FindViewById<ListView>(Resource.Id.maPharmacyTable);

			var add = FindViewById<ImageView>(Resource.Id.maAdd);
			add.Click += delegate {
				StartActivity(new Intent(this, typeof(PharmacyActivity)));
			};
		}

		protected override void OnResume()
		{
			base.OnResume();

			realm = Realm.GetInstance();
			pharmacies = realm.All<Pharmacy>().ToList();

			adapter = new PharmacyAdapter(this, pharmacies);

			listView.Adapter = adapter;
		}

		protected override void OnPause()
		{
			base.OnPause();

			listView.Adapter = null;
			adapter = null;
			pharmacies = null;

			realm.Dispose();
		}
	}
}


