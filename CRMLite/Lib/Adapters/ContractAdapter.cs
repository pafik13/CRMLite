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

			var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.ContractTableItem, parent, false)
			           ) as LinearLayout;

			view.SetTag(Resource.String.ContractDataUUID, ContractDatas[position].UUID);

			view.FindViewById<TextView>(Resource.Id.ctiNameTV).Text = item.name;
			view.FindViewById<TextView>(Resource.Id.ctiTimeTV).Text = @"<пусто>";
			view.FindViewById<TextView>(Resource.Id.ctiDescriptionTV).Text = item.description;
			if (!string.IsNullOrEmpty(item.FilePath)) {
				if (System.IO.File.Exists(item.FilePath)) {
					view.FindViewById<ImageView>(Resource.Id.ctiCanShowFileIV).SetImageResource(Resource.Drawable.ic_visibility_black_36dp);
				}
			}

			var del = view.FindViewById<RelativeLayout>(Resource.Id.ctiDeleteRL);
			del.Click -= Del_Click;
			del.Click += Del_Click;

			return view;
		}


		void Del_Click(object sender, System.EventArgs e)
		{
			if (sender is RelativeLayout) {
				var row = ((RelativeLayout)sender).Parent as LinearLayout;
				var contractDataUUID = (string)row.GetTag(Resource.String.ContractDataUUID);
				if (string.IsNullOrEmpty(contractDataUUID)) return;

				foreach (var item in ContractDatas) {
					if (item.UUID == contractDataUUID) {
						ContractDatas.Remove(item);
						break;
					}
				}

				MainDatabase.DeleteEntity<ContractData>(contractDataUUID);

				NotifyDataSetChanged();
			}
		}
	}
}

