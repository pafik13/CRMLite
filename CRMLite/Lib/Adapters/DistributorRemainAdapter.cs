using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class DistributorRemainAdapter : BaseAdapter<DistributorRemain>
	{
		readonly Activity Context;
		readonly IList<DistributorRemain> DistributorRemains;

		public DistributorRemainAdapter(Activity context, IList<DistributorRemain> distributorRemains)
		{
			Context = context;
			DistributorRemains = distributorRemains;
		}

		public override DistributorRemain this[int position] {
			get {
				return DistributorRemains[position];
			}
		}

		public override int Count {
			get {
				return DistributorRemains.Count;
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for positio
			var item = DistributorRemains[position];

			var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.DistributorRemainTableItem, parent, false)
					   ) as LinearLayout;

			//view.SetTag(Resource.String.ContractDataUUID, ContractDatas[position].UUID);

			view.FindViewById<TextView>(Resource.Id.drtiDistributorTV).Text = item.distributor;
			view.FindViewById<TextView>(Resource.Id.drtiDateTV).Text = item.date.LocalDateTime.ToString("dd.MM.yy");
			view.FindViewById<TextView>(Resource.Id.drtiRemainTV).Text = item.remain.ToString();

			return view;
		}
	}
}

