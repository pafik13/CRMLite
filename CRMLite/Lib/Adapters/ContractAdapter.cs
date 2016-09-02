using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class ContractAdapter : BaseAdapter<ContractData>
	{
		readonly Activity Context;
		readonly IList<ContractData> ContractDatas;

		public ContractAdapter(Activity context, IList<ContractData> contractDatas)
		{
			Context = context;
			ContractDatas = contractDatas;
		}

		public override ContractData this[int position] {
			get {
				return ContractDatas[position];
			}
		}

		public override int Count {
			get {
				return ContractDatas.Count;
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = MainDatabase.GetItem<Contract>(ContractDatas[position].Contract);

			var view = (convertView ??
								Context.LayoutInflater.Inflate(
								Resource.Layout.DistributionTableItem,
								parent,
								false)) as LinearLayout;

			//view.FindViewById<ImageView>(Resource.Id.ctiIsFilePresentsIV).SetImageDrawable();
			view.FindViewById<TextView>(Resource.Id.ctiNameTV).Text = item.name;
			view.FindViewById<TextView>(Resource.Id.ctiDescriptionTV).Text = item.description;

			return view;
		}
	}
}

