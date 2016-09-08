using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class HospitalDataAdapter : BaseAdapter<HospitalData>
	{
		readonly Activity Context;
		readonly IList<HospitalData> HospitalDatas;

		public HospitalDataAdapter(Activity context, IList<HospitalData> hospitalDatas)
		{
			Context = context;
			HospitalDatas = hospitalDatas;
		}

		public override HospitalData this[int position]
		{
			get { return HospitalDatas[position]; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { return HospitalDatas.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = HospitalDatas[position];

			var view = (convertView ??
			            Context.LayoutInflater.Inflate(Resource.Layout.HospitalTableItem, parent, false)
			           ) as LinearLayout;
			view.SetTag(Resource.String.HospitalDataUUID, item.UUID);

			if (string.IsNullOrEmpty(item.ListedHospital)) {
				var hospital = MainDatabase.GetEntity<Hospital>(item.Hospital);
				view.FindViewById<TextView>(Resource.Id.htiNameTV).Text = 
					string.IsNullOrEmpty(hospital.Name) ? "<пусто>" : hospital.Name;
				view.FindViewById<TextView>(Resource.Id.htiAddressTV).Text = 
					string.IsNullOrEmpty(hospital.Address) ? "<пусто>" : hospital.Address;
			} else {
				var listedHospital = MainDatabase.GetItem<ListedHospital>(item.ListedHospital);
				view.FindViewById<TextView>(Resource.Id.htiNameTV).Text =
					    string.IsNullOrEmpty(listedHospital.name) ? "<пусто>" : listedHospital.name;
				view.FindViewById<TextView>(Resource.Id.htiAddressTV).Text =
					    string.IsNullOrEmpty(listedHospital.address) ? "<пусто>" : listedHospital.address;				
			}

			var del = view.FindViewById<RelativeLayout>(Resource.Id.htiDeleteRL);
			del.Click -= Del_Click;
			del.Click += Del_Click;

			return view;
		}

		void Del_Click(object sender, System.EventArgs e)
		{
			if (sender is RelativeLayout) {
				var row = ((RelativeLayout)sender).Parent as LinearLayout;
				var hospitalDataUUID = (string)row.GetTag(Resource.String.HospitalDataUUID);
				if (string.IsNullOrEmpty(hospitalDataUUID)) return;

				foreach (var item in HospitalDatas) {
					if (item.UUID == hospitalDataUUID) {
						HospitalDatas.Remove(item);
						break;
					}
				}

				MainDatabase.DeleteEntity<HospitalData>(hospitalDataUUID);

				NotifyDataSetChanged();
			}
		}
	}
}