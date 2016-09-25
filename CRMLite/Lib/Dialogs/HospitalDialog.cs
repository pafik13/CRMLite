﻿using System;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;
using CRMLite.DaData;

namespace CRMLite.Dialogs
{
	public class HospitalDialog : DialogFragment
	{
		public const string TAG = "HospitalDialog";

		public event EventHandler AfterSaved;

		readonly Pharmacy Pharmacy;
		readonly HospitalData HospitalData;

		Hospital Hospital;
		SuggestClient Api;

		protected virtual void OnAfterSaved(EventArgs e)
		{
			if (AfterSaved != null)
			{
				AfterSaved(this, e);
			}
		}

		public HospitalDialog(Pharmacy pharmacy, HospitalData hospitalData = null)
		{
			if (pharmacy == null) {
				throw new ArgumentNullException(nameof(pharmacy));
			}
			Pharmacy = pharmacy;
			HospitalData = hospitalData;
			Api = new SuggestClient(Secret.DadataApiToken, Secret.DadataApiURL);
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			Dialog.SetCanceledOnTouchOutside(false);

			var caption = string.Empty;
			if (HospitalData == null)
			{
				caption = "НОВОЕ ЛПУ";
			} else {
				Hospital = MainDatabase.GetEntity<Hospital>(HospitalData.Hospital);
				caption = "ЛПУ : " + Hospital.Name;
			}

			Dialog.SetTitle(caption);

			View view = inflater.Inflate(Resource.Layout.HospitalDialog, container, false);

			view.FindViewById<EditText>(Resource.Id.hdNameET).Append(Hospital == null ? string.Empty : Hospital.Name);

			AutoCompleteTextView addressACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.hdAddressACTV);
			addressACTV.Append(Hospital == null ? @"Москва" : Hospital.Address);
			addressACTV.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) =>
			{
				if (addressACTV.Text.Contains(" "))
				{
					var response = Api.QueryAddress(addressACTV.Text);
					var suggestions = response.suggestionss.Select(x => x.value).ToArray();
					addressACTV.Adapter = new ArrayAdapter<string>(
						Activity, Android.Resource.Layout.SimpleDropDownItem1Line, suggestions
					);
					(addressACTV.Adapter as ArrayAdapter<string>).NotifyDataSetChanged();
					if (addressACTV.IsShown) {
						addressACTV.DismissDropDown();
					}
					addressACTV.ShowDropDown();
				}
			};

			view.FindViewById<Button>(Resource.Id.hdCloseB).Click += (s, e) => {
				Dismiss();
			};

			view.FindViewById<Button>(Resource.Id.hdSaveB).Click += (s, e) => {
				//Toast.MakeText(context, "SAVE BUTTON CLICKED", ToastLength.Short).Show()

				var transaction = MainDatabase.BeginTransaction();
				if (HospitalData == null) {
					var hospital = MainDatabase.Create<Hospital>();
					hospital.CreatedAt = DateTimeOffset.Now;
					hospital.UpdatedAt = DateTimeOffset.Now;
					hospital.Name = view.FindViewById<EditText>(Resource.Id.hdNameET).Text;
					hospital.Address = view.FindViewById<EditText>(Resource.Id.hdAddressACTV).Text;

					var hospitalData = MainDatabase.Create<HospitalData>();
					hospitalData.Pharmacy = Pharmacy.UUID;
					hospitalData.Hospital = hospital.UUID;
				} else {
					//var hospital = MainDatabase.GetEntity<Hospital>(HospitalData.Hospital);
					//hospital.UpdatedAt = DateTimeOffset.Now;
					//hospital.Name = view.FindViewById<EditText>(Resource.Id.hdNameET).Text;
					//hospital.Address = view.FindViewById<EditText>(Resource.Id.hdAddressACTV).Text;

					Hospital.UpdatedAt = DateTimeOffset.Now;
					Hospital.Name = view.FindViewById<EditText>(Resource.Id.hdNameET).Text;
					Hospital.Address = view.FindViewById<EditText>(Resource.Id.hdAddressACTV).Text;

					if (!Hospital.IsManaged) MainDatabase.SaveEntity(transaction, Hospital);
				}

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

