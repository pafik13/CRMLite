using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class ListedHospitalAdapter : BaseAdapter<ListedHospital>
	{
		readonly Activity Context;
		readonly IList<ListedHospital> ListedHospitals;

		public ListedHospitalAdapter(Activity context, IList<ListedHospital> listedHospitals)
		{
			Context = context;
			ListedHospitals = listedHospitals;
		}

		public override ListedHospital this[int position] {
			get { return ListedHospitals[position]; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get { return ListedHospitals.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = ListedHospitals[position];

			var view = (convertView ??
			            Context.LayoutInflater.Inflate(Resource.Layout.ListedHospitalTableItem, parent, false)
					   ) as LinearLayout;

			view.FindViewById<TextView>(Resource.Id.lhtiNameTV).Text =
					string.IsNullOrEmpty(item.name) ? "<пусто>" : item.name;
			view.FindViewById<TextView>(Resource.Id.lhtiAddressTV).Text =
					string.IsNullOrEmpty(item.address) ? "<пусто>" : item.address;

			return view;
		}
	}
}