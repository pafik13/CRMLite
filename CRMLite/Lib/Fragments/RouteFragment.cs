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

		DateTimeOffset BaseDate;


		DateTimeOffset MondayDate;
		ListView MondayTable;

		DateTimeOffset TuesdayDate;
		ListView TuesdayTable;

		DateTimeOffset WednesdayDate;
		ListView WednesdayTable;

		DateTimeOffset ThursdayDate;
		ListView ThursdayTable;

		DateTimeOffset FridayDate;
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
			var now = DateTime.Now;
			BaseDate = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, new TimeSpan(0, 0, 0));
			BaseDate = BaseDate.AddDays(7 * Offset);
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
			MondayDate = BaseDate.AddDays(-(int)BaseDate.DayOfWeek + (int)DayOfWeek.Monday);
			header.FindViewById<TextView>(Resource.Id.rwthDateTV).Text = MondayDate.Date.ToLongDateString();
			MondayTable.AddHeaderView(header);

			TuesdayTable = rootView.FindViewById<ListView>(Resource.Id.rwTuesdayTable);
			header = inflater.Inflate(Resource.Layout.RouteWeekTableHeader, TuesdayTable, false);
			TuesdayDate = BaseDate.AddDays(-(int)BaseDate.DayOfWeek + (int)DayOfWeek.Tuesday);
			header.FindViewById<TextView>(Resource.Id.rwthDateTV).Text = TuesdayDate.Date.ToLongDateString();
			TuesdayTable.AddHeaderView(header);

			WednesdayTable = rootView.FindViewById<ListView>(Resource.Id.rwWednesdayTable);
			header = inflater.Inflate(Resource.Layout.RouteWeekTableHeader, WednesdayTable, false);
			WednesdayDate = BaseDate.AddDays(-(int)BaseDate.DayOfWeek + (int)DayOfWeek.Wednesday);
			header.FindViewById<TextView>(Resource.Id.rwthDateTV).Text = WednesdayDate.Date.ToLongDateString();
			WednesdayTable.AddHeaderView(header);

			ThursdayTable = rootView.FindViewById<ListView>(Resource.Id.rwThursdayTable);
			header = inflater.Inflate(Resource.Layout.RouteWeekTableHeader, ThursdayTable, false);
			ThursdayDate = BaseDate.AddDays(-(int)BaseDate.DayOfWeek + (int)DayOfWeek.Thursday);
			header.FindViewById<TextView>(Resource.Id.rwthDateTV).Text = ThursdayDate.Date.ToLongDateString();
			ThursdayTable.AddHeaderView(header);

			FridayTable = rootView.FindViewById<ListView>(Resource.Id.rwFridayTable);
			header = inflater.Inflate(Resource.Layout.RouteWeekTableHeader, FridayTable, false);
			FridayDate = BaseDate.AddDays(-(int)BaseDate.DayOfWeek + (int)DayOfWeek.Friday);
			header.FindViewById<TextView>(Resource.Id.rwthDateTV).Text = FridayDate.Date.ToLongDateString();
			FridayTable.AddHeaderView(header);

			return rootView;
		}

		public override void OnResume()
		{
			base.OnResume();

			MondayTable.Adapter = new RouteDayInWeekAdapter(Activity, MainDatabase.GetRouteItems(MondayDate));
			TuesdayTable.Adapter = new RouteDayInWeekAdapter(Activity, MainDatabase.GetRouteItems(TuesdayDate));
			WednesdayTable.Adapter = new RouteDayInWeekAdapter(Activity, MainDatabase.GetRouteItems(WednesdayDate));
			ThursdayTable.Adapter = new RouteDayInWeekAdapter(Activity, MainDatabase.GetRouteItems(ThursdayDate));
			FridayTable.Adapter = new RouteDayInWeekAdapter(Activity, MainDatabase.GetRouteItems(FridayDate));
		}
	}
}
