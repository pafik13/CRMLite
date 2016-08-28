using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class ContractAdapter : BaseAdapter<ContractData>
	{
		Activity Context;
		readonly IList<ContractData> ContractDatas;

		public ContractAdapter(Activity context, IList<ContractData> contractDatas) : base()
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
			var item = ContractDatas[position];

			var view = (convertView ??
								Context.LayoutInflater.Inflate(
								Resource.Layout.DistributionTableItem,
								parent,
								false)) as LinearLayout;

			//view.FindViewById<TextView>(Resource.Id.dtiDrugSKUTV).Text = MainDatabase.GetItem<DrugSKU>(item.DrugSKU).name;
			//view.FindViewById<CheckBox>(Resource.Id.dtiIsExistenceCB).Checked = item.IsExistence;
			//view.FindViewById<EditText>(Resource.Id.dtiCountET).Text = item.Count.ToString();
			//view.FindViewById<EditText>(Resource.Id.dtiPriceET).Text = item.Price.ToString();
			//view.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked = item.IsPresence;
			//view.FindViewById<EditText>(Resource.Id.dtiOrderET).Text = item.Order;
			//view.FindViewById<EditText>(Resource.Id.dtiCommentET).Text = item.Comment;

			return view;
		}
	}
}

