using System;
using System.Linq;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class DistributorRemainAdapter : BaseAdapter<Distributor>
	{
		readonly Activity Context;
		readonly IList<Distributor> Distributors;
		readonly IList<DateTimeOffset> Dates;

		public DistributorRemainAdapter(Activity context, IList<Distributor> distributors, IList<DateTimeOffset> dates)
		{
			Context = context;
			Distributors = distributors;
			Dates = dates;
		}

		public override Distributor this[int position] {
			get {
				return Distributors[position];
			}
		}

		public override int Count {
			get {
				return Distributors.Count;
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for positio
			var item = Distributors[position];

			var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.DistributorRemainTableItem, parent, false)
					   ) as LinearLayout;
			 
			var distributorRemains = MainDatabase.GetItems<DistributorRemain>().Where(dr => dr.distributor == item.uuid);
			var remains = new Dictionary<DateTimeOffset, DistributorRemain>();
			foreach (var remain in distributorRemains) {
				remains.Add(remain.date, remain);
			}

			foreach (var date in Dates) {
				var txt = new TextView(Context);
				if (remains.ContainsKey(date)) {
					txt.Text = remains[date].remain.ToString();
				}
				view.AddView(txt);
			}

			return view;
		}
	}
}

