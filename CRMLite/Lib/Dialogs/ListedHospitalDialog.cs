using System;
using System.Linq;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;
using CRMLite.Adapters;
using System.Globalization;

namespace CRMLite.Dialogs
{
	public class ListedHospitalDialog : DialogFragment
	{
		public const string TAG = "ListedHospitalDialog";
		public const int C_DISPLAYED_ITEMS = 8;

		public event EventHandler AfterSaved;

		readonly Pharmacy Pharmacy;

		TextView ResultNone;
		ListView ResultTable;
		IList<ListedHospital> ListedHospitalsExeptPicked;
		List<ListedHospital> Displayed;
		CultureInfo Culture;

		protected virtual void OnAfterSaved(EventArgs e)
		{
			if (AfterSaved != null) {
				AfterSaved(this, e);
			}
		}

		public ListedHospitalDialog(Pharmacy pharmacy)
		{
			if (pharmacy == null) {
				throw new ArgumentNullException(nameof(pharmacy));
			}
			Pharmacy = pharmacy;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			Dialog.SetCanceledOnTouchOutside(true);

			var caption = @"ПОИСК ЛПУ";

			Dialog.SetTitle(caption);

			View view = inflater.Inflate(Resource.Layout.ListedHospitalDialog, container, false);

			Culture = CultureInfo.GetCultureInfo("ru-RU");

			ResultTable = view.FindViewById<ListView>(Resource.Id.lhdResultLV);
			ResultNone = view.FindViewById<TextView>(Resource.Id.lhdResultTV);

			var listedHospitals = MainDatabase.GetItems<ListedHospital>() ?? new List<ListedHospital>();

			var pickedUUIDs = new List<string>();
			foreach (var item in MainDatabase.GetItems<HospitalData>() ?? new List<HospitalData>()) {
				if (string.IsNullOrEmpty(item.ListedHospital)) continue;
				pickedUUIDs.Add(item.ListedHospital);
			}

			ListedHospitalsExeptPicked = listedHospitals.Where(lh => !pickedUUIDs.Contains(lh.uuid)).ToList();

			if (ListedHospitalsExeptPicked.Count == 0) {
				ResultNone.Visibility = ViewStates.Visible;
			} else {
				Displayed = ListedHospitalsExeptPicked.Take(C_DISPLAYED_ITEMS).ToList();

				ResultTable.Adapter = new ListedHospitalAdapter(Activity, Displayed);
				ResultNone.Visibility = ViewStates.Invisible;
			}

			view.FindViewById<EditText>(Resource.Id.lhdSearchET).AfterTextChanged += (sender, e) => {
				var src = e.Editable.ToString();

				if (ListedHospitalsExeptPicked.Count == 0) {
					ResultNone.Visibility = ViewStates.Visible;
					return;
				}

				if (string.IsNullOrEmpty(src)) {
					Displayed = ListedHospitalsExeptPicked.Take(C_DISPLAYED_ITEMS).ToList();

					ResultTable.Adapter = new ListedHospitalAdapter(Activity, Displayed);
					ResultNone.Visibility = ViewStates.Invisible;
					return;
				}

				Displayed = ListedHospitalsExeptPicked.Where(ld => Culture.CompareInfo.IndexOf(ld.name, src, CompareOptions.IgnoreCase) >= 0
				                                              || Culture.CompareInfo.IndexOf(ld.address, src, CompareOptions.IgnoreCase) >= 0
				                                             )
				                                       .Take(C_DISPLAYED_ITEMS)
				                                       .ToList();

				if (Displayed.Count > 0) {
					ResultTable.Adapter = new ListedHospitalAdapter(Activity, Displayed);
					ResultNone.Visibility = ViewStates.Invisible;
				} else {
					ResultNone.Visibility = ViewStates.Visible;
				}
			};

			ResultTable.ItemClick += (sender, e) => {

				//Toast.MakeText(context, "SAVE BUTTON CLICKED", ToastLength.Short).Show()
				var item = Displayed[e.Position];

				var transaction = MainDatabase.BeginTransaction();

				var hospitalData = MainDatabase.Create2<HospitalData>();
				hospitalData.Pharmacy = Pharmacy.UUID;
				hospitalData.ListedHospital = item.uuid;

				transaction.Commit();
				//var sync = new SyncItem()
				//{
				//	Path = @"Hospital",
				//	ObjectUUID = hospital.UUID,
				//	JSON = JsonConvert.SerializeObject(hospital)
				//};

				//MainDatabase.AddToQueue(sync);

				//Context.StartService(new Intent("com.xamarin.SyncService"));

				OnAfterSaved(EventArgs.Empty);

				Dismiss();
			};

			return view;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
		}
	}
}

