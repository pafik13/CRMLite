using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class DistributionAdapter : BaseAdapter<DistributionData>
	{
		Activity Context;
		readonly IList<DistributionData> Distributions;

		public DistributionAdapter(Activity context, IList<DistributionData> distributions) : base()
		{
			Context = context;
			Distributions = distributions ?? new List<DistributionData>();
		}

		public override DistributionData this[int position]
		{
			get
			{
				return Distributions[position];
			}
		}

		public override int Count
		{
			get
			{
				return Distributions.Count;
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{

			// Get our object for position
			var item = Distributions[position];

			var view = (convertView ??
								Context.LayoutInflater.Inflate(
								Resource.Layout.DistributionTableItem,
								parent,
								false)) as LinearLayout;

			view.FindViewById<TextView>(Resource.Id.dtiDrugSKUTV).Text = MainDatabase.GetItem<DrugSKU>(item.DrugSKU).name;
			view.FindViewById<CheckBox>(Resource.Id.dtiIsExistenceCB).Checked = item.IsExistence;
			view.FindViewById<EditText>(Resource.Id.dtiCountET).Text = item.Count.ToString();
			view.FindViewById<EditText>(Resource.Id.dtiPriceET).Text = item.Price.ToString();
			view.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked = item.IsPresence;
			view.FindViewById<EditText>(Resource.Id.dtiOrderET).Text = item.Order;
			view.FindViewById<EditText>(Resource.Id.dtiCommentET).Text = item.Comment;

			return view;
		}
	}
}

