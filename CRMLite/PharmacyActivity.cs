using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using Realms;

using Newtonsoft.Json;

using CRMLite.Entities;
using CRMLite.Suggestions;

namespace CRMLite
{
	[Activity(Label = "PharmacyActivity")]
	public class PharmacyActivity : Activity
	{
		Pharmacy pharmacy;
		Transaction transaction;

		Spinner State;
		IList<string> states;

		Spinner Net;
		List<Net> nets;

		List<Subway> subways;
		List<Region> regions;
		List<Place> places;
		List<Category> byNet;
		List<Category> bySell;


		SuggestClient Api;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Pharmacy);

			var token = Secret.DadataApiToken;
			var url = "https://suggestions.dadata.ru/suggestions/api/4_1/rs";
			Api = new SuggestClient(token, url);

			transaction = MainDatabase.BeginTransaction();

			var pharmacyUUID = Intent.GetStringExtra("UUID");
			var caption = "ДОБАВЛЕНИЕ НОВОЙ АПТЕКИ";
			if (string.IsNullOrEmpty(pharmacyUUID))
			{
				pharmacy = MainDatabase.CreatePharmacy();
				pharmacy.LegalAddress = "Москва";
				pharmacy.Address = "Москва";
			}
			else
			{
				pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
				var result = MainDatabase.GetSyncResult(pharmacyUUID);
				if (result != null)
				{
					pharmacy.LastSyncResult = result;
				}
				caption = "АПТЕКА : " + pharmacy.GetName();

				if (pharmacy.LastSyncResult != null)
				{
					caption += string.Format(" (синхр. {0} в {1})"
					                         , pharmacy.LastSyncResult.createdAt.ToLocalTime().ToString("dd.MM.yy")
					                         , pharmacy.LastSyncResult.createdAt.ToLocalTime().ToString("HH:mm:ss")
					                        );
				}
			}
			// Screen caption set
			FindViewById<TextView>(Resource.Id.paInfoTV).Text = caption;

			FindViewById<TextView>(Resource.Id.paUUIDTV).Text = pharmacy.UUID;

			#region State
			State = FindViewById<Spinner>(Resource.Id.paStateS);
			states = MainDatabase.GetStates();
			ArrayAdapter stateAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, states.ToArray());
			//stateAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			State.Adapter = stateAdapter;
			State.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) =>
			{
				pharmacy.SetState((PharmacyState)e.Position);
			};
			// SetValue
			State.SetSelection((int)pharmacy.GetState());
			#endregion

			FindViewById<EditText>(Resource.Id.paNameET).Text = pharmacy.Name;
			FindViewById<EditText>(Resource.Id.paNameInNetET).Text = pharmacy.NameInNet;

			#region LegalAddress
			var legalAddressACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paLegalAddressACTV);
			legalAddressACTV.Text = pharmacy.LegalAddress;

			legalAddressACTV.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) =>
			{
				if (legalAddressACTV.Text.Contains(" "))
				{
					var response = Api.QueryAddress(legalAddressACTV.Text);
					var suggestions = response.suggestionss.Select(x => x.value).ToArray();
					legalAddressACTV.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, suggestions);
					(legalAddressACTV.Adapter as ArrayAdapter<string>).NotifyDataSetChanged();
					if (legalAddressACTV.IsShown)
					{
						legalAddressACTV.DismissDropDown();
					}
					legalAddressACTV.ShowDropDown();
				}
			};
			#endregion

			#region Net
			Net = FindViewById<Spinner>(Resource.Id.paNetS);
			nets = MainDatabase.GetNets();
			ArrayAdapter netAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, (from item in nets select item.name).ToArray());
			//stateAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			Net.Adapter = netAdapter;
			Net.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) =>
			{
				pharmacy.Net = nets[e.Position].uuid;
			};
			// SetValue
			if (!string.IsNullOrEmpty(pharmacy.Net)) {
				Net.SetSelection(nets.FindIndex(item => string.Compare(item.uuid, pharmacy.Net) == 0)); 
			};
			#endregion

			#region Address
			var addressACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paAddressACTV);
			addressACTV.Text = pharmacy.Address;

			addressACTV.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) =>
			{
				if (addressACTV.Text.Contains(" "))
				{
					var response = Api.QueryAddress(addressACTV.Text);
					var suggestions = response.suggestionss.Select(x => x.value).ToArray();
					addressACTV.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, suggestions);
					(addressACTV.Adapter as ArrayAdapter<string>).NotifyDataSetChanged();
					if (addressACTV.IsShown)
					{
						addressACTV.DismissDropDown();
					}
					addressACTV.ShowDropDown();
				}
			};
			#endregion

			#region Subway
			AutoCompleteTextView subwayACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paSubwayACTV);
			subwayACTV.Text = string.IsNullOrEmpty(pharmacy.Subway) ? pharmacy.Subway : MainDatabase.GetSubway(pharmacy.Subway).name;
			//string[] stations = Resources.GetStringArray(Resource.Array.moscow_stations);
			subways = MainDatabase.GetSubways();
			subwayACTV.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, subways.Select(x => x.name).ToArray());
			subwayACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				pharmacy.Subway = subways[e.Position].uuid;
			};
			#endregion

			#region Region
			AutoCompleteTextView regionACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paRegionACTV);
			regionACTV.Text = string.IsNullOrEmpty(pharmacy.Region) ? pharmacy.Region : MainDatabase.GetRegion(pharmacy.Region).name;
			//string[] regions = Resources.GetStringArray(Resource.Array.moscow_regions);
			regions = MainDatabase.GetRegions();
			regionACTV.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, regions.Select(x => x.name).ToArray());
			regionACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				pharmacy.Region = regions[e.Position].uuid;
			};
			#endregion

			FindViewById<EditText>(Resource.Id.paPhoneET).Text = pharmacy.Phone;

			#region Place
			AutoCompleteTextView placeACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paPlaceACTV);
			placeACTV.Text = string.IsNullOrEmpty(pharmacy.Place) ? pharmacy.Place : MainDatabase.GetPlace(pharmacy.Place).name;
			//string[] places = Resources.GetStringArray(Resource.Array.pharmacy_places);
			places = MainDatabase.GetPlaces();
			placeACTV.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, places.Select(x => x.name).ToArray());
			placeACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				pharmacy.Place = places[e.Position].uuid;
			};
			#endregion

			#region CategoryByNet
			AutoCompleteTextView byNetACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryByNetACTV);
			byNetACTV.Text = string.IsNullOrEmpty(pharmacy.CategoryByNet) ? pharmacy.CategoryByNet : MainDatabase.GetCategory(pharmacy.CategoryByNet).name;
			byNet = MainDatabase.GetCategories("net");
			byNetACTV.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, byNet.Select(x => x.name).ToArray());
			byNetACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				pharmacy.CategoryByNet = byNet[e.Position].uuid;
			};
			#endregion

			#region CategoryBySell
			AutoCompleteTextView bySellACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryBySellACTV);
			bySellACTV.Text = string.IsNullOrEmpty(pharmacy.CategoryBySell) ? pharmacy.CategoryBySell : MainDatabase.GetCategory(pharmacy.CategoryBySell).name;
			bySell = MainDatabase.GetCategories("sell");
			bySellACTV.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, bySell.Select(x => x.name).ToArray());
			bySellACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				pharmacy.CategoryBySell = bySell[e.Position].uuid;
			};
			#endregion


			FindViewById<EditText>(Resource.Id.paCommentET).Text = pharmacy.Comment;


			var close = FindViewById<Button>(Resource.Id.paCloseB);
			close.Click += delegate {
				if (pharmacy.CreatedAt == null)
				{
					MainDatabase.DeletePharmacy(pharmacy);
				}

				transaction.Commit();
				Finish();
			};

			var save = FindViewById<Button>(Resource.Id.paSaveB);
			save.Click += delegate {
				pharmacy.CreatedAt = DateTimeOffset.Now;
				pharmacy.Name = FindViewById<EditText>(Resource.Id.paNameET).Text;
				pharmacy.NameInNet = FindViewById<EditText>(Resource.Id.paNameInNetET).Text;
				pharmacy.LegalAddress = FindViewById<AutoCompleteTextView>(Resource.Id.paLegalAddressACTV).Text;
				pharmacy.Address = FindViewById<AutoCompleteTextView>(Resource.Id.paAddressACTV).Text;
				//pharmacy.Subway = FindViewById<AutoCompleteTextView>(Resource.Id.paSubwayACTV).Text;
				//pharmacy.Region = FindViewById<AutoCompleteTextView>(Resource.Id.paRegionACTV).Text;
				pharmacy.Phone = FindViewById<EditText>(Resource.Id.paPhoneET).Text;
				//pharmacy.Place = FindViewById<AutoCompleteTextView>(Resource.Id.paPlaceACTV).Text;
				//pharmacy.CategoryByNet = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryByNetACTV).Text;
				//pharmacy.CategoryBySell = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryBySellACTV).Text;
				pharmacy.Comment = FindViewById<EditText>(Resource.Id.paCommentET).Text;

				transaction.Commit();

				var sync = new SyncItem()
				{
					Path = @"Pharmacy",
					ObjectUUID = pharmacy.UUID,
					JSON = JsonConvert.SerializeObject(pharmacy)
				};

				MainDatabase.AddToQueue(sync);

				StartService(new Intent("com.xamarin.SyncService"));

				Finish();
			};



		}
	}
}

