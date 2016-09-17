using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class AttendanceByWeekAdapter : BaseAdapter<Dictionary<int, int>>
	{
		readonly Activity Context;
		readonly IList<RouteItem> RouteItems;
        readonly Dictionary<string, Dictionary<int, int>> Items;  // PharmacyUUID - YearWeek (Year * 100 + Week) - Count
        readonly int[] YearWeeks;

		public AttendanceByWeekAdapter(Activity context, Dictionary<string, Dictionary<int, int>> items, DateTimeOffset[] dates)
		{
			Context = context;
			Items = items;

            YearWeeks = new int[14]
            for (int d = 0; d < 14; d++)
            {
                YearWeeks[d] = dates[d].Year * 100 + dates[d].Week;
            }
		}

		public override Dictionary<int, int> this[int position] {
			get {
				return Items[Items.Keys[position]];
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get {
				return Items.Keys.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
            var key = Items.Keys[position];
			var pharmacy = MainDatabase.GetEntity<Pharmacy>(key);
            var item = Items[key];
            view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.AttendanceByWeekTableItem, parent, false)
                   ) as LinearLayout; 

            view.FindViewById<TextView>(Resource.Id.abwtiPharmacyTV).Text = pharmacy.GetName();

            view.FindViewById<TextView>(Resource.Id.abwtiWeek1).Text = item[YearWeeks[0]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek2).Text = item[YearWeeks[1]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek3).Text = item[YearWeeks[2]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek4).Text = item[YearWeeks[3]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek5).Text = item[YearWeeks[4]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek6).Text = item[YearWeeks[5]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek7).Text = item[YearWeeks[6]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek8).Text = item[YearWeeks[7]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek9).Text = item[YearWeeks[8]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek10).Text = item[YearWeeks[9]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek11).Text = item[YearWeeks[10]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek12).Text = item[YearWeeks[11]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek13).Text = item[YearWeeks[12]];
            view.FindViewById<TextView>(Resource.Id.abwtiWeek14).Text = item[YearWeeks[13]];

            return view;
		}
	}
}

