using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class HospitalAdapter : BaseAdapter<Hospital>
	{
		Activity context = null;
		readonly IList<Hospital> hospitals = new List<Hospital>();

		public HospitalAdapter(Activity context, IList<Hospital> hospitals) : base()
		{
			this.context = context;
			this.hospitals = hospitals;
		}

		public override Hospital this[int position]
		{
			get { return hospitals[position]; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { return hospitals.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = hospitals[position];

			var view = (convertView ??
								context.LayoutInflater.Inflate(
				            	Resource.Layout.HospitalTableItem,
								parent,
								false)) as LinearLayout;

			view.FindViewById<TextView>(Resource.Id.htiNameTV).Text = string.IsNullOrEmpty(item.Name) ? "<пусто>" : item.Name;
			view.FindViewById<TextView>(Resource.Id.htiAddressTV).Text = string.IsNullOrEmpty(item.Address) ? "<пусто>" : item.Address;

			return view;
		}
	}
}