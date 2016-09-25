using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class MessageAdapter: BaseAdapter<Message>
	{
		readonly Activity Context;
		readonly IList<Message> Messages;

		public MessageAdapter(Activity context, IList<Message> messages)
		{
			Context = context;
			Messages = messages;
		}

		public override Message this[int position] {
			get {
				return Messages[position];
			}
		}

		public override int Count {
			get {
				return Messages.Count;
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for positio
			var item = Messages[position];

			var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.MessageTableItem, parent, false)
					   ) as LinearLayout;

			//view.SetTag(Resource.String.ContractDataUUID, ContractDatas[position].UUID);

			view.FindViewById<TextView>(Resource.Id.mtiMessageTV).Text = item.Text;
			view.FindViewById<TextView>(Resource.Id.mtiCreatedAtTV).Text = item.CreatedAt.LocalDateTime.ToString("dd.MM.yy");
			view.FindViewById<CheckBox>(Resource.Id.mtiIsSyncedCB).Checked = item.IsSynced;


			//view.FindViewById<TextView>(Resource.Id.ctiDescriptionTV).Text = item.description;
			//if (!string.IsNullOrEmpty(item.FilePath)) {
			//	if (System.IO.File.Exists(item.FilePath)) {
			//		view.FindViewById<ImageView>(Resource.Id.ctiCanShowFileIV).SetImageResource(Resource.Drawable.ic_visibility_black_36dp);
			//	}
			//}

			//var del = view.FindViewById<RelativeLayout>(Resource.Id.ctiDeleteRL);
			//del.Click -= Del_Click;
			//del.Click += Del_Click;

			return view;
		}


		//void Del_Click(object sender, System.EventArgs e)
		//{
		//	if (sender is RelativeLayout) {
		//		var row = ((RelativeLayout)sender).Parent as LinearLayout;
		//		var contractDataUUID = (string)row.GetTag(Resource.String.ContractDataUUID);
		//		if (string.IsNullOrEmpty(contractDataUUID)) return;

		//		foreach (var item in ContractDatas) {
		//			if (item.UUID == contractDataUUID) {
		//				ContractDatas.Remove(item);
		//				break;
		//			}
		//		}

		//		MainDatabase.DeleteEntity<ContractData>(contractDataUUID);

		//		NotifyDataSetChanged();
		//	}
		//}
	}
}
