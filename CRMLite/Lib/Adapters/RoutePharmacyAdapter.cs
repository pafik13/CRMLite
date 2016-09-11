using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class RoutePharmacyAdapter : BaseAdapter<SearchItem>
	{
		readonly Activity Context;
		readonly IList<SearchItem> SearchItems;

		public RoutePharmacyAdapter(Activity context, IList<SearchItem> searchItems) : base()
		{
			Context = context;
			SearchItems = searchItems;
		}

		public override SearchItem this[int position] {
			get {
				return SearchItems[position];
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get {
				return SearchItems.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = SearchItems[position];

			var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.RoutePharmacyItem, parent, false)
					   ) as LinearLayout;

			view.FindViewById<TextView>(Resource.Id.sriPharmacyTV).Text = item.Name;

			if (string.IsNullOrEmpty(item.Match)) {
				view.FindViewById<TextView>(Resource.Id.sriMatchTV).Visibility = ViewStates.Gone;
			} else {
				view.FindViewById<TextView>(Resource.Id.sriMatchTV).Text = item.Match;
			}

			//Finally return the view
			return view;
		}
	}
}

