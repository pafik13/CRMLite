using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class RouteDayInWeekAdapter : BaseAdapter<RouteItem>
	{
		readonly Activity Context;
		readonly IList<RouteItem> RouteItems;

		public RouteDayInWeekAdapter(Activity context, IList<RouteItem> routeItems)
		{
			Context = context;
			RouteItems = routeItems;
		}

		public override RouteItem this[int position] {
			get {
				return RouteItems[position];
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get {
				return RouteItems.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var pharmacy = MainDatabase.GetEntity<Pharmacy>(RouteItems[position].Pharmacy);
            
            var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.RouteWeekTableItem, parent, false)
			           ) as LinearLayout; 

            view.FindViewById<TextView>(Resource.Id.rwtiPharmacyTV).Text = pharmacy.GetName();
            
            if (string.IsNullOrEmpty(pharmacy.Subway)) {
                if (string.IsNullOrEmpty(pharmacy.Region)) {
                    return view;
                }
				view.FindViewById<TextView>(Resource.Id.rwtiSubwayOrRegionTV).Text = MainDatabase.GetItem<Region>(pharmacy.Region).name;
				return view;
            }
			view.FindViewById<TextView>(Resource.Id.rwtiSubwayOrRegionTV).Text = MainDatabase.GetItem<Subway>(pharmacy.Subway).name;
            return view;
		}
	}
}

