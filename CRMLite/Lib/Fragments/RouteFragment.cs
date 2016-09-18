using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using CRMLite.Adapters;

namespace CRMLite
{
	public class RouteFragment: Fragment
	{
		public const string C_OFFSET = @"C_OFFSET";

		int Offset;

		DateTimeOffset Date;

		ListView MondayTable;
		ListView TuesdayTable;
		ListView WednesdayTable;
		ListView ThursdayTable;
		ListView FridayTable;


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
			Date = DateTimeOffset.Now.AddDays(7*Offset);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);

			// Inflate the layout
			var rootView = (ViewGroup) inflater.Inflate(Resource.Layout.RouteWeek, container, false);

			View header;
			// Find tables
			MondayTable = rootView.FindViewById<ListView>(Resource.Id.rwMondayTable);
			header = inflater.Inflate(Resource.Layout.RouteWeekTableHeader, MondayTable, false);
			header.FindViewById<TextView>(Resource.Id.rwthDateTV).Text = Date.AddDays(-(int)DateTimeOffset.Now.DayOfWeek + (int)DayOfWeek.Monday).LocalDateTime.ToLongDateString();
			MondayTable.AddHeaderView(header);

			TuesdayTable = rootView.FindViewById<ListView>(Resource.Id.rwTuesdayTable);
			header = inflater.Inflate(Resource.Layout.RouteWeekTableHeader, TuesdayTable, false);
			header.FindViewById<TextView>(Resource.Id.rwthDateTV).Text = Date.AddDays(-(int)DateTimeOffset.Now.DayOfWeek + (int)DayOfWeek.Tuesday).LocalDateTime.ToLongDateString();
			TuesdayTable.AddHeaderView(header);

			WednesdayTable = rootView.FindViewById<ListView>(Resource.Id.rwWednesdayTable);
			header = inflater.Inflate(Resource.Layout.RouteWeekTableHeader, WednesdayTable, false);
			header.FindViewById<TextView>(Resource.Id.rwthDateTV).Text = Date.AddDays(-(int)DateTimeOffset.Now.DayOfWeek + (int)DayOfWeek.Wednesday).LocalDateTime.ToLongDateString();
			WednesdayTable.AddHeaderView(header);

			ThursdayTable = rootView.FindViewById<ListView>(Resource.Id.rwThursdayTable);
			header = inflater.Inflate(Resource.Layout.RouteWeekTableHeader, ThursdayTable, false);
			header.FindViewById<TextView>(Resource.Id.rwthDateTV).Text = Date.AddDays(-(int)DateTimeOffset.Now.DayOfWeek + (int)DayOfWeek.Thursday).LocalDateTime.ToLongDateString();
			ThursdayTable.AddHeaderView(header);

			FridayTable = rootView.FindViewById<ListView>(Resource.Id.rwFridayTable);
			header = inflater.Inflate(Resource.Layout.RouteWeekTableHeader, FridayTable, false);
			header.FindViewById<TextView>(Resource.Id.rwthDateTV).Text = Date.AddDays(-(int)DateTimeOffset.Now.DayOfWeek + (int)DayOfWeek.Friday).LocalDateTime.ToLongDateString();
			FridayTable.AddHeaderView(header);

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
