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
		DateTimeOffset Date;
		DateTimeOffset[5] Dates;

		LinearLayout MondayTable;
		LinearLayout TuesdayTable;
		LinearLayout WednesdayTable;
		LinearLayout ThursdayTable;
		LinearLayout FridayTable;


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
			Date = DateTimeOffset.UtcNow.(AddDays(7*Offset));
			// Dates = new DateTimeOffset[5];
			// var d = Date.UtcDateTime.Date;
			// Dates[0] = d.AddDays(-(int)d.DayOfWeek + (int)DayOfWeek.Monday);
			// Dates[1] = d.AddDays(-(int)d.DayOfWeek + (int)DayOfWeek.Tuesday);
			// Dates[2] = d.AddDays(-(int)d.DayOfWeek + (int)DayOfWeek.Wednesday);
			// Dates[3] = d.AddDays(-(int)d.DayOfWeek + (int)DayOfWeek.Thursday);
			// Dates[4] = d.AddDays(-(int)d.DayOfWeek + (int)DayOfWeek.Friday);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);
			// Inflater = inflater;

			// Inflate the layout
			ViewGroup rootView = (ViewGroup)inflater
					.Inflate(Resource.Layout.RouteWeek, container, false);
			
			// Find tables
			MondayTable = rootView.FindByViewId<LinearLayout>(Resource.Id.rwMondayTable);
			TuesdayTable = rootView.FindByViewId<LinearLayout>(Resource.Id.rwTuesdayTable);
			WednesdayTable = rootView.FindByViewId<LinearLayout>(Resource.Id.rwWednesdayTable);
			ThursdayTable = rootView.FindByViewId<LinearLayout>(Resource.Id.rwThursdayTable);
			FridayTable = rootView.FindByViewId<LinearLayout>(Resource.Id.rwFridayTable);

			return rootView;
		}

		public override void OnResume()
		{
			base.OnResume();

			MondayTable.Adapter = new RouteDayInWeekAdapter(Activity, MainDatabase.GetRouteItems(Date, DayOfWeek.Monday));
			TuesdayTable.Adapter = new RouteDayInWeekAdapter(Activity, MainDatabase.GetRouteItems(Date, DayOfWeek.Tuesday));
			WednesdayTable.Adapter = new RouteDayInWeekAdapter(Activity, MainDatabase.GetRouteItems(Date, DayOfWeek.Wednesday));
			ThursdayTable.Adapter = new RouteDayInWeekAdapter(Activity, MainDatabase.GetRouteItems(Date, DayOfWeek.Thursday));
			FridayTable.Adapter = new RouteDayInWeekAdapter(Activity, MainDatabase.GetRouteItems(Date, DayOfWeek.Friday));
		}
	}
}
