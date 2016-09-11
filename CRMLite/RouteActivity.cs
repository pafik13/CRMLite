
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;
using CRMLite.Adapters;

namespace CRMLite
{
	[Activity(Label = "RouteActivity", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape)]
	public class RouteActivity : Activity
	{
		List<SearchItem> SearchItems;
		ListView PharmacyTable;
		LinearLayout RouteTable;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
    
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Route);

			PharmacyTable = FindViewById<ListView>(Resource.Id.raPharmacyTable);
			PharmacyTable.ItemClick += (sender, e) => {
				var row = LayoutInflater.Inflate(Resource.Layout.RoutePharmacyItem, RouteTable, false);

				var item = SearchItems[e.Position];
				row.FindViewById<TextView>(Resource.Id.sriPharmacyTV).Text = item.Name;
				RouteTable.AddView(row);
			};

			SearchItems = new List<SearchItem>();

			var pharmacies = MainDatabase.GetItems<Pharmacy>();
			foreach (var item in pharmacies) {
				SearchItems.Add(
					new SearchItem(
						item.UUID,
						item.GetName(),
						MainDatabase.GetItem<Subway>(item.Subway).name,
						MainDatabase.GetItem<Region>(item.Region).name,
						item.Brand
					)
				);
			}

			RouteTable = FindViewById<LinearLayout>(Resource.Id.raRouteTable);
		}

		protected override void OnResume()
		{
			base.OnResume();

			PharmacyTable.Adapter = new RoutePharmacyAdapter(this, SearchItems);
		}
	}
}

