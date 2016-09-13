using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using CRMLite.Entities;

namespace CRMLite
{
	public class RouteFragment: Fragment
	{
		public const string C_OFFSET = @"C_OFFSET";

		int Offset;
		LinearLayout RouteTable;
		LayoutInflater Inflater;
		/**
		 * Factory method for this fragment class. Constructs a new fragment for the given page number.
		 */
		public static RouteFragment create(int offset)
		{
			RouteFragment fragment = new RouteFragment();
			Bundle args = new Bundle();
			args.PutInt(C_OFFSET, offset);
			fragment.Arguments = args;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment heree
			Offset = Arguments.GetInt(C_OFFSET);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);
			Inflater = inflater;
			// Inflate the layout containing a title and body text.
			ViewGroup rootView = (ViewGroup)inflater
					.Inflate(Resource.Layout.RouteFragment, container, false);

			// Set the title view to show the page number.
			RouteTable = rootView.FindViewById<LinearLayout>(Resource.Id.rfRouteTable);

			return rootView;
		}

		public override void OnResume()
		{
			base.OnResume();

			RouteTable.RemoveAllViews();
			foreach (var item in MainDatabase.GetRouteItems(DateTimeOffset.UtcNow.AddDays(-Offset))) {

				var row = Inflater.Inflate(Resource.Layout.RouteItem, RouteTable, false);

				row.FindViewById<TextView>(Resource.Id.riPharmacyTV).Text = MainDatabase.GetEntity<Pharmacy>(item.Pharmacy).GetName();
				row.FindViewById<TextView>(Resource.Id.riOrderTV).Text = (item.Order + 1).ToString();

				RouteTable.AddView(row);
			}
		}
	}
}
