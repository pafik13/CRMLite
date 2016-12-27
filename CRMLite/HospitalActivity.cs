using System;
using System.Collections.Generic;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Content.PM;

using CRMLite.Entities;
using CRMLite.Adapters;
using CRMLite.Dialogs;

namespace CRMLite
{
	[Activity(Label = "HospitalActivity", ScreenOrientation = ScreenOrientation.Landscape)]
	public class HospitalActivity : Activity
	{
		public const string C_PHARMACY_UUID = @"C_PHARMACY_UUID";

		Pharmacy Pharmacy;
		IList<HospitalData> HospitalDatas;
		ListView HospitalTable;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Hospital);

			FindViewById<Button>(Resource.Id.haCloseB).Click += (s, e) => {
				Finish();
			};

			var pharmacyUUID = Intent.GetStringExtra(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return;

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
			FindViewById<TextView>(Resource.Id.haInfoTV).Text = string.Format("ЛПУ БЛИЗКИЕ К АПТЕКЕ : {0}", Pharmacy.GetName());

			HospitalDatas = MainDatabase.GetPharmacyDatas<HospitalData>(Pharmacy.UUID);

			HospitalTable = FindViewById<ListView>(Resource.Id.haHospitalTable);

			var header = LayoutInflater.Inflate(Resource.Layout.HospitalTableHeader, HospitalTable, false);
			HospitalTable.AddHeaderView(header);

			HospitalTable.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				HospitalData item;
				if (HospitalTable.HeaderViewsCount > 0) {
					if (e.Position < HospitalTable.HeaderViewsCount) {
						return;
					}
					item = HospitalDatas[e.Position - HospitalTable.HeaderViewsCount];
				} else {
					item = HospitalDatas[e.Position];
				}

				if (string.IsNullOrEmpty(item.Hospital)) {
					Toast.MakeText(this, @"Нельзя редактировать данные о ЛПУ, которые пришли с сервера!", ToastLength.Short).Show();
					return;
				}

				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(HospitalDialog.TAG);
				if (prev != null) {
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var hospitalDialog = new HospitalDialog(Pharmacy, item);
				hospitalDialog.Show(fragmentTransaction, HospitalDialog.TAG);
				hospitalDialog.AfterSaved += (caller, arguments) => {
					Console.WriteLine("Event {0} was called", "AfterSaved");

					RecreateAdapter();
				};
			};


			FindViewById<ImageView>(Resource.Id.haAddIV).Click += (s, ea) => {
				Console.WriteLine("Event {0} was called", "haAdd_Click");

				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(HospitalDialog.TAG);
				if (prev != null) {
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var hospitalDialog = new HospitalDialog(Pharmacy);
				hospitalDialog.Show(fragmentTransaction, HospitalDialog.TAG);
				hospitalDialog.AfterSaved += (object sender, EventArgs e) =>
				{
					Console.WriteLine("Event {0} was called", "AfterSaved");

					RecreateAdapter();
				};
			};


			FindViewById<ImageView>(Resource.Id.haListIV).Click += (s, ea) => {
				Console.WriteLine("Event {0} was called", "haListIV_Click");

				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(ListedHospitalDialog.TAG);
				if (prev != null) {
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var hospitalDialog = new ListedHospitalDialog(Pharmacy);
				hospitalDialog.Show(fragmentTransaction, ListedHospitalDialog.TAG);
				hospitalDialog.AfterSaved += (object sender, EventArgs e) => {
					Console.WriteLine("Event {0} was called", "ListedHospitalDialog_AfterSaved");

					RecreateAdapter();
				};
			};
		}

		protected override void OnResume()
		{
			base.OnResume();

			if (Pharmacy == null) {
				new AlertDialog.Builder(this)
								   .SetTitle(Resource.String.error_caption)
								   .SetMessage("Отсутствует аптека!")
								   .SetCancelable(false)
								   .SetPositiveButton(@"OK", (dialog, args) => {
									   if (dialog is Dialog) {
										   ((Dialog)dialog).Dismiss();
									   }
								   })
								   .Show();

			} else {
				Helper.CheckIfTimeChangedAndShowDialog(this);

				RecreateAdapter();
			}
		}

		void RecreateAdapter()
		{
			HospitalDatas = MainDatabase.GetPharmacyDatas<HospitalData>(Pharmacy.UUID);

			HospitalTable.Adapter = new HospitalDataAdapter(this, HospitalDatas);
		}

		protected override void OnPause()
		{
			base.OnPause();
		}

		protected override void OnStop()
		{
			base.OnStop();
		}
	}
}


