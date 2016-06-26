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

		IList<Pharmacy> pharmacies = new List<Pharmacy>();
		ListView listView;
		PharmacyAdapter adapter;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			if (Secret.IsNeedReCreateDB) {
				Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
			}

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			listView = FindViewById<ListView>(Resource.Id.maPharmacyTable);
			View header = LayoutInflater.Inflate(Resource.Layout.PharmacyTableHeader, listView, false);
			listView.AddHeaderView(header);
			listView.Clickable = true;
			listView.ItemClick += OnListItemClick;

			var add = FindViewById<ImageView>(Resource.Id.maAdd);
			add.Click += delegate {
				StartActivity(new Intent(this, typeof(PharmacyActivity)));
			};
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			if (e.Position > 0)
			{
				var showPharmacy = new Intent(this, typeof(PharmacyActivity));
				showPharmacy.PutExtra(@"UUID", pharmacies[e.Position - 1].UUID);
				StartActivity(showPharmacy);
			}
		}

		protected override void OnResume()
		{
			base.OnResume();

			//realm = Realm.GetInstance();

			pharmacies = MainDatabase.GetPharmacies();//realm.All<Pharmacy>().ToList();

			adapter = new PharmacyAdapter(this, pharmacies);

			listView.Adapter = adapter;
		}

		protected override void OnPause()
		{
			base.OnPause();

			//listView.Adapter = null;
			//adapter = null;
			//pharmacies = null;

			//realm.Dispose();
		}

		protected override void OnStop()
		{
			base.OnStop();

			listView.Adapter = null;
			adapter = null;
			pharmacies = null;

			//realm.Dispose();
		}
	}
}


