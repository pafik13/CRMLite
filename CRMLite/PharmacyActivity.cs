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
	[Activity(Label = "PharmacyActivity", WindowSoftInputMode=SoftInput.StateHidden)]
	public class PharmacyActivity : Activity
	{
		Pharmacy pharmacy;
		Transaction transaction;

		Spinner State;
		IList<string> states;

		IList<Net> Nets;
		string NetUUID;
		AutoCompleteTextView NetName;

		IList<Contract> Contracts;
		AutoCompleteTextView ContractsNames;
		Button ContractsChoice;

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


			FindViewById<EditText>(Resource.Id.paBrandET).Text = pharmacy.Brand;
			FindViewById<EditText>(Resource.Id.paNumberNameET).Text = pharmacy.NumberName;
			FindViewById<EditText>(Resource.Id.paLegalNameET).Text = pharmacy.LegalName;


			#region Net
			Nets = MainDatabase.GetNets();
			NetName = FindViewById<AutoCompleteTextView>(Resource.Id.paNetACTV);
			if (!string.IsNullOrEmpty(pharmacy.Net)) {
				NetName.Text = MainDatabase.GetNet(pharmacy.Net).name;
			}
			var netChoiceButton = FindViewById<Button>(Resource.Id.paNetB);
			netChoiceButton.Click += (object sender, EventArgs e) => {
				new AlertDialog.Builder(this)
				               .SetTitle("Аптечная сеть")
							   .SetCancelable(true)
							   .SetItems(Nets.Select(item => item.name).ToArray(), (caller, arguments) => {
									SetNet(arguments.Which);
								   //Toast.MakeText(this, @"Selected " + arguments.Which, ToastLength.Short).Show();
							   })
							   .Show();
			};
			#endregion

			ContractsNames = FindViewById<AutoCompleteTextView>(Resource.Id.paContractsACTV);
			ContractsChoice = FindViewById<Button>(Resource.Id.paContractsB);

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

			#region Category
			AutoCompleteTextView byNetACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryACTV);
			//byNetACTV.Text = string.IsNullOrEmpty(pharmacy.CategoryByNet) ? pharmacy.CategoryByNet : MainDatabase.GetCategory(pharmacy.CategoryByNet).name;
			//byNet = MainDatabase.GetCategories("net");
			//byNetACTV.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, byNet.Select(x => x.name).ToArray());
			//byNetACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			//{
			//	pharmacy.CategoryByNet = byNet[e.Position].uuid;
			//};
			#endregion

			#region TurnOver
			AutoCompleteTextView turnOverACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paTurnOverACTV);
			//bySellACTV.Text = string.IsNullOrEmpty(pharmacy.CategoryBySell) ? pharmacy.CategoryBySell : MainDatabase.GetCategory(pharmacy.CategoryBySell).name;
			//bySell = MainDatabase.GetCategories("sell");
			//bySellACTV.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, bySell.Select(x => x.name).ToArray());
			//bySellACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			//{
			//	pharmacy.CategoryBySell = bySell[e.Position].uuid;
			//};
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
				pharmacy.Brand = FindViewById<EditText>(Resource.Id.paBrandET).Text;
				pharmacy.NumberName = FindViewById<EditText>(Resource.Id.paNumberNameET).Text;
				pharmacy.LegalName = FindViewById<EditText>(Resource.Id.paLegalNameET).Text;
				pharmacy.Address = FindViewById<AutoCompleteTextView>(Resource.Id.paAddressACTV).Text;
				//pharmacy.Subway = FindViewById<AutoCompleteTextView>(Resource.Id.paSubwayACTV).Text;
				//pharmacy.Region = FindViewById<AutoCompleteTextView>(Resource.Id.paRegionACTV).Text;
				pharmacy.Phone = FindViewById<EditText>(Resource.Id.paPhoneET).Text;
				//pharmacy.Place = FindViewById<AutoCompleteTextView>(Resource.Id.paPlaceACTV).Text;
				//pharmacy.CategoryByNet = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryByNetACTV).Text;
				//pharmacy.CategoryBySell = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryBySellACTV).Text;
				pharmacy.Comment = FindViewById<EditText>(Resource.Id.paCommentET).Text;

				transaction.Commit();

				//var sync = new SyncItem()
				//{
				//	Path = @"Pharmacy",
				//	ObjectUUID = pharmacy.UUID,
				//	JSON = JsonConvert.SerializeObject(pharmacy)
				//};

				//MainDatabase.AddToQueue(sync);

				//StartService(new Intent("com.xamarin.SyncService"));

				Finish();
			};



		}

		void SetNet(int index)
		{
			if ((Nets != null) && (Nets.Count > 0)) {
				if (NetUUID != Nets[index].uuid) {
					NetUUID = Nets[index].uuid;
					NetName.Text = Nets[index].name;

					// Init Contracts
					ContractsNames.Text = string.Empty;
					Contracts = MainDatabase.GetItems<Contract>().Where(c => c.net == NetUUID).ToList();
					ContractsChoice.Enabled = (Contracts.Count > 0);

					ContractsChoice.Click -= ContractsChoice_Click;
					ContractsChoice.Click += ContractsChoice_Click;
				}
			}
		}

		void ContractsChoice_Click(object sender, EventArgs e)
		{
			new AlertDialog.Builder(this)
			               .SetTitle("Контракты")
						   .SetCancelable(true)
						   .SetItems(Contracts.Select(item => item.name).ToArray(), (caller, arguments) => {
							   ContractsNames.Text = Contracts[arguments.Which].name;
						   })
						   .Show();
		}
	}
}

