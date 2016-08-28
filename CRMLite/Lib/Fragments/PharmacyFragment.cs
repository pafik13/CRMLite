using System;
using System.Collections.Generic;
using System.Linq;

using Android.OS;
using Android.Views;
using Android.Widget;

using Android.Support.V4.App;

using CRMLite.Dialogs;
using CRMLite.Entities;
using CRMLite.Suggestions;

namespace CRMLite
{
	public class PharmacyFragment : Fragment
	{
		public const string ARG_UUID = @"ARG_UUID";

		Pharmacy pharmacy;

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

		public static PharmacyFragment create(string UUID)
		{
			PharmacyFragment fragment = new PharmacyFragment();
			Bundle arguments = new Bundle();
			arguments.PutString(ARG_UUID, UUID);
			fragment.Arguments = arguments;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);

			View view = inflater.Inflate(Resource.Layout.PharmacyFragment, container, false);

			var token = Secret.DadataApiToken;
			var url = "https://suggestions.dadata.ru/suggestions/api/4_1/rs";
			Api = new SuggestClient(token, url);

			var pharmacyUUID = Arguments.GetString(ARG_UUID);
			var caption = "ДОБАВЛЕНИЕ НОВОЙ АПТЕКИ";
			if (string.IsNullOrEmpty(pharmacyUUID))
			{
				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(LockDialog.TAG);
				if (prev != null)
				{
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var lockDialog = LockDialog.Create(@"МОЙ ПЕРВЫЙ ЛОКЕР");
				lockDialog.Cancelable = false;
				lockDialog.Show(fragmentTransaction, LockDialog.TAG);
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
			//view.FindViewById<TextView>(Resource.Id.pfInfoTV).Text = caption;

			view.FindViewById<TextView>(Resource.Id.pfUUIDTV).Text = pharmacy.UUID;

			#region State
			states = MainDatabase.GetStates();
			var state = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfStateACTV);
			state.Text = states[(int)pharmacy.GetState()];
			var stateChoiceButton = view.FindViewById<Button>(Resource.Id.pfStateB);
			stateChoiceButton.Click += (object sender, EventArgs e) =>
			{
				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Статус аптеки")
				           .SetItems(states.ToArray(), (caller, arguments) => {
							   Toast.MakeText(Activity, @"Selected " + arguments.Which, ToastLength.Short).Show();
							})
				           .Show();
			};
			#endregion

			//view.FindViewById<EditText>(Resource.Id.pfNameET).Text = pharmacy.Name;
			//view.FindViewById<EditText>(Resource.Id.pfNameInNetET).Text = pharmacy.NameInNet;

			#region LegalAddress
			var legalAddressACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfLegalAddressACTV);
			//legalAddressACTV.Text = pharmacy.LegalAddress;

			legalAddressACTV.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) =>
			{
				if (legalAddressACTV.Text.Contains(" "))
				{
					var response = Api.QueryAddress(legalAddressACTV.Text);
					var suggestions = response.suggestionss.Select(x => x.value).ToArray();
					legalAddressACTV.Adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleDropDownItem1Line, suggestions);
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
			nets = MainDatabase.GetNets();
			var net = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfNetACTV);
			if (!string.IsNullOrEmpty(pharmacy.Net))
			{
				net.Text = MainDatabase.GetNet(pharmacy.Net).name;
			}
			var netChoiceButton = view.FindViewById<Button>(Resource.Id.pfNetB);
			netChoiceButton.Click += (object sender, EventArgs e) =>
			{
				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Аптечная сеть")
				           .SetCancelable(true)
						   .SetItems(nets.Select(item => item.name).ToArray(), (caller, arguments) =>
						   {
							   Toast.MakeText(Activity, @"Selected " + arguments.Which, ToastLength.Short).Show();
						   })
						   .Show();
			};
			#endregion

			#region Address
			var addressACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfAddressACTV);
			addressACTV.Text = pharmacy.Address;

			addressACTV.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) =>
			{
				if (addressACTV.Text.Contains(" "))
				{
					var response = Api.QueryAddress(addressACTV.Text);
					var suggestions = response.suggestionss.Select(x => x.value).ToArray();
					addressACTV.Adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleDropDownItem1Line, suggestions);
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
			AutoCompleteTextView subwayACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfSubwayACTV);
			//subwayACTV.Text = string.IsNullOrEmpty(pharmacy.Subway) ? pharmacy.Subway : MainDatabase.GetSubway(pharmacy.Subway).name;
			//string[] stations = Resources.GetStringArray(Resource.Array.moscow_stations);
			//subways = MainDatabase.GetSubways();
			//subwayACTV.Adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleDropDownItem1Line, subways.Select(x => x.name).ToArray());
			subwayACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				pharmacy.Subway = subways[e.Position].uuid;
			};
			#endregion

			#region Region
			AutoCompleteTextView regionACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfRegionACTV);
			//regionACTV.Text = string.IsNullOrEmpty(pharmacy.Region) ? pharmacy.Region : MainDatabase.GetRegion(pharmacy.Region).name;
			//string[] regions = Resources.GetStringArray(Resource.Array.moscow_regions);
			//regions = MainDatabase.GetRegions();
			//regionACTV.Adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleDropDownItem1Line, regions.Select(x => x.name).ToArray());
			regionACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				pharmacy.Region = regions[e.Position].uuid;
			};
			#endregion

			view.FindViewById<EditText>(Resource.Id.pfPhoneET).Text = pharmacy.Phone;

			#region Place
			AutoCompleteTextView placeACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfPlaceACTV);
			placeACTV.Text = string.IsNullOrEmpty(pharmacy.Place) ? pharmacy.Place : MainDatabase.GetPlace(pharmacy.Place).name;
			//string[] places = Resources.GetStringArray(Resource.Array.pharmacy_places);
			places = MainDatabase.GetPlaces();
			placeACTV.Adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleDropDownItem1Line, places.Select(x => x.name).ToArray());
			placeACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				pharmacy.Place = places[e.Position].uuid;
			};
			#endregion

			#region CategoryByNet
			AutoCompleteTextView byNetACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfCategoryByNetACTV);
			//byNetACTV.Text = string.IsNullOrEmpty(pharmacy.CategoryByNet) ? pharmacy.CategoryByNet : MainDatabase.GetCategory(pharmacy.CategoryByNet).name;
			//byNet = MainDatabase.GetCategories("net");
			//byNetACTV.Adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleDropDownItem1Line, byNet.Select(x => x.name).ToArray());
			//byNetACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			//{
			//	pharmacy.CategoryByNet = byNet[e.Position].uuid;
			//};
			#endregion

			#region CategoryBySell
			AutoCompleteTextView bySellACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfCategoryBySellACTV);
			//bySellACTV.Text = string.IsNullOrEmpty(pharmacy.CategoryBySell) ? pharmacy.CategoryBySell : MainDatabase.GetCategory(pharmacy.CategoryBySell).name;
			//bySell = MainDatabase.GetCategories("sell");
			//bySellACTV.Adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleDropDownItem1Line, bySell.Select(x => x.name).ToArray());
			//bySellACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			//{
			//	pharmacy.CategoryBySell = bySell[e.Position].uuid;
			//};
			#endregion


			view.FindViewById<EditText>(Resource.Id.pfCommentET).Text = pharmacy.Comment;

			return view;
		}
	}
}

