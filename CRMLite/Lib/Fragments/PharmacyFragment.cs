using System;
using System.Collections.Generic;
using System.Linq;

using Android.OS;
using Android.Views;
using Android.Widget;

using Android.Support.V4.App;
using CRMLite.Entities;
using CRMLite.DaData;
using Realms;

namespace CRMLite
{
	public class PharmacyFragment : Fragment, IAttendanceControl
	{
		public const string C_PHARMACY_UUID = @"C_PHARMACY_UUID";

		Pharmacy Pharmacy;

		Spinner State;
		IList<string> States;

		IList<Net> Nets;
		string NetUUID;
		AutoCompleteTextView NetName;

		IList<Contract> Contracts;
		AutoCompleteTextView ContractsNames;
		Button ContractsChoice;

		IList<Subway> Subways;
		IList<Region> Regions;
		IList<Place> Places;
		IList<Category> CategoryByNets;


		SuggestClient Api;

		public static PharmacyFragment create(string pharmacyUUID)
		{
			PharmacyFragment fragment = new PharmacyFragment();
			Bundle arguments = new Bundle();
			arguments.PutString(C_PHARMACY_UUID, pharmacyUUID);
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

			var pharmacyUUID = Arguments.GetString(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return view;

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);

			view.FindViewById<TextView>(Resource.Id.pfUUIDTV).Text = Pharmacy.UUID;


			#region State
			State = view.FindViewById<Spinner>(Resource.Id.pfStateS);
			States = MainDatabase.GetStates();
			var stateAdapter = new ArrayAdapter(Context, Android.Resource.Layout.SimpleSpinnerItem, States.ToArray());
			//stateAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			State.Adapter = stateAdapter;
			//State.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {
			//	Pharmacy.SetState((PharmacyState)e.Position);
			//};
			// SetValue
			State.SetSelection((int)Pharmacy.GetState());
			#endregion

			view.FindViewById<EditText>(Resource.Id.pfBrandET).Text = Pharmacy.Brand;
			view.FindViewById<EditText>(Resource.Id.pfNumberNameET).Text = Pharmacy.NumberName;
			view.FindViewById<EditText>(Resource.Id.pfLegalNameET).Text = Pharmacy.LegalName;


			#region Net
			Nets = MainDatabase.GetNets();
			NetName = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfNetACTV);
			if (!string.IsNullOrEmpty(Pharmacy.Net)) {
				NetName.Text = MainDatabase.GetNet(Pharmacy.Net).name;
			}
			var netChoiceButton = view.FindViewById<Button>(Resource.Id.pfNetB);
			netChoiceButton.Click += (object sender, EventArgs e) => {
				new Android.App.AlertDialog.Builder(Context)
							   .SetTitle("Аптечная сеть")
							   .SetCancelable(true)
							   .SetItems(Nets.Select(item => item.name).ToArray(), (caller, arguments) => {
								   //SetNet(arguments.Which);
								   //Toast.MakeText(this, @"Selected " + arguments.Which, ToastLength.Short).Show();
							   })
							   .Show();
			};
			#endregion

			ContractsNames = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfContractsACTV);
			ContractsChoice = view.FindViewById<Button>(Resource.Id.pfContractsB);

			#region Address
			var addressACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfAddressACTV);
			addressACTV.Text = Pharmacy.Address;

			addressACTV.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) => {
				if (addressACTV.Text.Contains(" ")) {
					var response = Api.QueryAddress(addressACTV.Text);
					var suggestions = response.suggestionss.Select(x => x.value).ToArray();
					addressACTV.Adapter = new ArrayAdapter<string>(
						Context, Android.Resource.Layout.SimpleDropDownItem1Line, suggestions
					);
					(addressACTV.Adapter as ArrayAdapter<string>).NotifyDataSetChanged();
					if (addressACTV.IsShown) {
						addressACTV.DismissDropDown();
					}
					addressACTV.ShowDropDown();
				}
			};
			#endregion

			#region Subway
			AutoCompleteTextView subwayACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfSubwayACTV);
			subwayACTV.Text = string.IsNullOrEmpty(Pharmacy.Subway) ?
				string.Empty : MainDatabase.GetItem<Subway>(Pharmacy.Subway).name;
			Subways = MainDatabase.GetItems<Subway>();
			subwayACTV.Adapter = new ArrayAdapter<string>(
				Context, Android.Resource.Layout.SimpleDropDownItem1Line, Subways.Select(s => s.name).ToArray()
			);
			subwayACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Pharmacy.Subway = Subways[e.Position].uuid;
			};
			#endregion

			#region Region
			AutoCompleteTextView regionACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfRegionACTV);
			regionACTV.Text = string.IsNullOrEmpty(Pharmacy.Region) ?
				string.Empty : MainDatabase.GetItem<Region>(Pharmacy.Region).name;
			Regions = MainDatabase.GetItems<Region>();
			regionACTV.Adapter = new ArrayAdapter<string>(
				Context, Android.Resource.Layout.SimpleDropDownItem1Line, Regions.Select(r => r.name).ToArray()
			);
			regionACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Pharmacy.Region = Regions[e.Position].uuid;
			};
			#endregion

			view.FindViewById<EditText>(Resource.Id.pfPhoneET).Text = Pharmacy.Phone;

			#region Place
			AutoCompleteTextView placeACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfPlaceACTV);
			placeACTV.Text = string.IsNullOrEmpty(Pharmacy.Place) ?
				string.Empty : MainDatabase.GetItem<Place>(Pharmacy.Place).name;
			Places = MainDatabase.GetItems<Place>();
			placeACTV.Adapter = new ArrayAdapter<string>(
				Context, Android.Resource.Layout.SimpleDropDownItem1Line, Places.Select(x => x.name).ToArray()
			);
			placeACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Pharmacy.Place = Places[e.Position].uuid;
			};
			#endregion

			#region Category
			AutoCompleteTextView byNetACTV = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfCategoryACTV);
			byNetACTV.Text = string.IsNullOrEmpty(Pharmacy.Category) ?
				string.Empty : MainDatabase.GetItem<Category>(Pharmacy.Category).name;
			CategoryByNets = MainDatabase.GetCategories("net");
			byNetACTV.Adapter = new ArrayAdapter<string>(
				Context, Android.Resource.Layout.SimpleDropDownItem1Line, CategoryByNets.Select(c => c.name).ToArray()
			);
			byNetACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Pharmacy.Category = CategoryByNets[e.Position].uuid;
			};
			#endregion

			view.FindViewById<EditText>(Resource.Id.pfTurnOverET).Text = Pharmacy.TurnOver.HasValue ?
				Pharmacy.TurnOver.Value.ToString() : string.Empty;

			view.FindViewById<TextView>(Resource.Id.pfLastAttendanceTV).Text = Pharmacy.LastAttendanceDate.HasValue ?
				Pharmacy.LastAttendanceDate.Value.ToString(@"dd.MM.yyyy") : @"<нет визита>";

			view.FindViewById<TextView>(Resource.Id.pfNextAttendanceDateTV).Text = Pharmacy.NextAttendanceDate.HasValue ?
				Pharmacy.NextAttendanceDate.Value.ToString(@"dd.MM.yyyy") : DateTimeOffset.Now.ToString(@"dd.MM.yyyy");

			view.FindViewById<EditText>(Resource.Id.pfCommentET).Text = Pharmacy.Comment;

			return view;
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
			new Android.App.AlertDialog.Builder(Context)
						   .SetTitle("Контракты")
						   .SetCancelable(true)
						   .SetItems(Contracts.Select(item => item.name).ToArray(), (caller, arguments) => {
							   ContractsNames.Text = Contracts[arguments.Which].name;
						   })
						   .Show();
		}

		public void OnAttendanceStart(DateTimeOffset? start)
		{
			if (!start.HasValue) {
				throw new ArgumentNullException(nameof(start));
			}
		}

		public void OnAttendanceStop(Transaction openedTransaction, Attendance current)
		{
			if (openedTransaction == null) {
				throw new ArgumentNullException(nameof(openedTransaction));
			}

			if (current == null) {
				throw new ArgumentNullException(nameof(current));
			}

			var pharmacy = MainDatabase.GetEntity<Pharmacy>(Pharmacy.UUID);
			pharmacy.LastAttendance = current.UUID;
			pharmacy.LastAttendanceDate = current.When;
			// TODO: Сделать глобальной настройкой
			pharmacy.NextAttendanceDate = current.When.AddDays(14);
		}
	}
}

