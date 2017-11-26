using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class RoutePharmacyAdapter : BaseAdapter<RouteSearchItem>
	{
		readonly Activity Context;
		readonly IList<RouteSearchItem> RouteSearchItems;
		readonly IList<string> PharmacyStates;
		public RoutePharmacyAdapter(Activity context, IList<RouteSearchItem> routeSearchItems)
		{
			Context = context;
			RouteSearchItems = routeSearchItems;
			PharmacyStates = MainDatabase.GetStates();
		}

		public override RouteSearchItem this[int position] {
			get {
				return RouteSearchItems[position];
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get {
				return RouteSearchItems.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = RouteSearchItems[position];

			if (item.IsVisible) {
				var isValidView = (convertView is LinearLayout);
				View view;
				if (isValidView) {
					view = convertView as LinearLayout;
				} else {
					view = Context.LayoutInflater.Inflate(Resource.Layout.RoutePharmacyItem, parent, false); 
				}

				view.FindViewById<TextView>(Resource.Id.sriPharmacyTV).Text = item.Name;

				var stateTV = view.FindViewById<TextView>(Resource.Id.sriPharmacyStateTV);
				var state = item.State.ToEnum(PharmacyState.psClose);
				switch (state) {
					case PharmacyState.psActive:
						stateTV.Text = PharmacyStates[(int)PharmacyState.psActive];
						stateTV.SetTextAppearance(Context, Resource.Style.pharmacyStateActive);
						break;
					case PharmacyState.psReserve:
						stateTV.Text = PharmacyStates[(int)PharmacyState.psReserve];
						stateTV.SetTextAppearance(Context, Resource.Style.pharmacyStateReserve);
						break;
					case PharmacyState.psClose:
						stateTV.Text = PharmacyStates[(int)PharmacyState.psClose];
						stateTV.SetTextAppearance(Context, Resource.Style.pharmacyStateClose);
						break;
				}

				if (string.IsNullOrEmpty(item.Match)) {
					view.FindViewById<TextView>(Resource.Id.sriMatchTV).Visibility = ViewStates.Gone;
				} else {
					view.FindViewById<TextView>(Resource.Id.sriMatchTV).Text = item.Match;
				}

				//Finally, return the view
				return view;
			}

			return new View(Context);
		}

		public void SwitchVisibility(int position)
		{
			var item = RouteSearchItems[position];

			item.IsVisible = !item.IsVisible;

			NotifyDataSetChanged();
		}

		public void ChangeVisibility(int position, bool isVisible)
		{
			var item = RouteSearchItems[position];

			item.IsVisible = isVisible;

			NotifyDataSetChanged();
		}
	}
}

