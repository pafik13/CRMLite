using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class PharmacyAdapter : BaseAdapter<Pharmacy>
	{
		Activity context = null;
		IList<Pharmacy> pharmacies = new List<Pharmacy>();

		public PharmacyAdapter(Activity context, IList<Pharmacy> pharmacies) : base()
		{
			this.context = context;
			this.pharmacies = pharmacies;
		}

		public override Pharmacy this[int position]
		{
			get { return pharmacies[position]; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { return pharmacies.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = pharmacies[position];

			var view = (convertView ??
								context.LayoutInflater.Inflate(
				            	Resource.Layout.PharmacyTableItem,
								parent,
								false)) as LinearLayout;
			
			view.FindViewById<TextView>(Resource.Id.ptiNameTV).Text = string.IsNullOrEmpty(item.LegalName) ? "<unknow name>" : item.LegalName;
			view.FindViewById<TextView>(Resource.Id.ptiAddressTV).Text = string.IsNullOrEmpty(item.Address) ? "<unknow address>" : item.Address;

			//view.FindViewById<Button>(Resource.Id.ptiLastAttendanceDateB).Click += delegate {
			//	Toast.MakeText(context, "Нажали на кнопку!", ToastLength.Short).Show();
			//};

			var showEmploee = view.FindViewById<ImageView>(Resource.Id.ptiEmploeeIV);
			showEmploee.SetTag(Resource.String.PharmacyUUID, item.UUID);
			showEmploee.Click -= ShowEmploeeClickEventHandler;
			showEmploee.Click += ShowEmploeeClickEventHandler;

			//Finally return the view
			return view;
		}

		void ShowEmploeeClickEventHandler(object sender, System.EventArgs e)
		{
			if (sender is ImageView)
			{
				var pharmacyUUID = ((ImageView)sender).GetTag(Resource.String.PharmacyUUID).ToString();
				var emploeeAcivity = new Intent(context, typeof(EmploeeActivity));
				emploeeAcivity.PutExtra(@"UUID", pharmacyUUID);
				context.StartActivity(emploeeAcivity);
			}
		}
	}
}
