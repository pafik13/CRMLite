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
		Pharmacy Pharmacy;
		Transaction transaction;

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
				Pharmacy = MainDatabase.CreatePharmacy();
				Pharmacy.Address = "Москва";
			}
			else
			{
				Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
				var result = MainDatabase.GetSyncResult(pharmacyUUID);
				if (result != null)
				{
					Pharmacy.LastSyncResult = result;
				}
				caption = "АПТЕКА : " + Pharmacy.GetName();

				if (Pharmacy.LastSyncResult != null)
				{
					caption += string.Format(" (синхр. {0} в {1})"
					                         , Pharmacy.LastSyncResult.createdAt.ToLocalTime().ToString("dd.MM.yy")
					                         , Pharmacy.LastSyncResult.createdAt.ToLocalTime().ToString("HH:mm:ss")
					                        );
				}
			}
			// Screen caption set
			FindViewById<TextView>(Resource.Id.paInfoTV).Text = caption;

			FindViewById<TextView>(Resource.Id.paUUIDTV).Text = Pharmacy.UUID;


			#region State
			State = FindViewById<Spinner>(Resource.Id.paStateS);
			States = MainDatabase.GetStates();
			ArrayAdapter stateAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, States.ToArray());
			//stateAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			State.Adapter = stateAdapter;
			State.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) =>
			{
				Pharmacy.SetState((PharmacyState)e.Position);
			};
			// SetValue
			State.SetSelection((int)Pharmacy.GetState());
			#endregion


			FindViewById<EditText>(Resource.Id.paBrandET).Text = Pharmacy.Brand;
			FindViewById<EditText>(Resource.Id.paNumberNameET).Text = Pharmacy.NumberName;
			FindViewById<EditText>(Resource.Id.paLegalNameET).Text = Pharmacy.LegalName;


			#region Net
			Nets = MainDatabase.GetNets();
			NetName = FindViewById<AutoCompleteTextView>(Resource.Id.paNetACTV);
			if (!string.IsNullOrEmpty(Pharmacy.Net)) {
				NetName.Text = MainDatabase.GetNet(Pharmacy.Net).name;
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
			addressACTV.Text = Pharmacy.Address;

			addressACTV.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) =>
			{
				if (addressACTV.Text.Contains(" "))
				{
					var response = Api.QueryAddress(addressACTV.Text);
					var suggestions = response.suggestionss.Select(x => x.value).ToArray();
					addressACTV.Adapter = new ArrayAdapter<string>(
						this, Android.Resource.Layout.SimpleDropDownItem1Line, suggestions
					);
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
			subwayACTV.Text = string.IsNullOrEmpty(Pharmacy.Subway) ? 
				string.Empty : MainDatabase.GetItem<Subway>(Pharmacy.Subway).name;
			Subways = MainDatabase.GetItems<Subway>();
			subwayACTV.Adapter = new ArrayAdapter<string>(
				this, Android.Resource.Layout.SimpleDropDownItem1Line, Subways.Select(s => s.name).ToArray()
			);
			subwayACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				Pharmacy.Subway = Subways[e.Position].uuid;
			};
			#endregion

			#region Region
			AutoCompleteTextView regionACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paRegionACTV);
			regionACTV.Text = string.IsNullOrEmpty(Pharmacy.Region) ? 
				string.Empty : MainDatabase.GetItem<Region>(Pharmacy.Region).name;
			Regions = MainDatabase.GetItems<Region>();
			regionACTV.Adapter = new ArrayAdapter<string>(
				this, Android.Resource.Layout.SimpleDropDownItem1Line, Regions.Select(r => r.name).ToArray()
			);
			regionACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				Pharmacy.Region = Regions[e.Position].uuid;
			};
			#endregion

			FindViewById<EditText>(Resource.Id.paPhoneET).Text = Pharmacy.Phone;

			#region Place
			AutoCompleteTextView placeACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paPlaceACTV);
			placeACTV.Text = string.IsNullOrEmpty(Pharmacy.Place) ?
				string.Empty : MainDatabase.GetItem<Place>(Pharmacy.Place).name;
			Places = MainDatabase.GetItems<Place>();
			placeACTV.Adapter = new ArrayAdapter<string>(
				this, Android.Resource.Layout.SimpleDropDownItem1Line, Places.Select(x => x.name).ToArray()
			);
			placeACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				Pharmacy.Place = Places[e.Position].uuid;
			};
			#endregion

			#region Category
			AutoCompleteTextView byNetACTV = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryACTV);
			byNetACTV.Text = string.IsNullOrEmpty(Pharmacy.Category) ? 
				string.Empty : MainDatabase.GetItem<Category>(Pharmacy.Category).name;
			CategoryByNets = MainDatabase.GetCategories("net");
			byNetACTV.Adapter = new ArrayAdapter<string>(
				this, Android.Resource.Layout.SimpleDropDownItem1Line, CategoryByNets.Select(c => c.name).ToArray()
			);
			byNetACTV.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
			{
				Pharmacy.Category = CategoryByNets[e.Position].uuid;
			};
			#endregion

			FindViewById<EditText>(Resource.Id.paTurnOverET).Text = Pharmacy.TurnOver.HasValue ?
				Pharmacy.TurnOver.Value.ToString() : string.Empty;

			FindViewById<TextView>(Resource.Id.paLastAttendanceTV).Text = Pharmacy.LastAttendanceDate.HasValue ?
				Pharmacy.LastAttendanceDate.Value.ToString(@"dd.MM.yyyy") : @"<нет визита>";

			FindViewById<TextView>(Resource.Id.paNextAttendanceDateTV).Text = Pharmacy.NextAttendanceDate.HasValue ?
				Pharmacy.NextAttendanceDate.Value.ToString(@"dd.MM.yyyy") : DateTimeOffset.Now.ToString(@"dd.MM.yyyy");

			FindViewById<EditText>(Resource.Id.paCommentET).Text = Pharmacy.Comment;


			var close = FindViewById<Button>(Resource.Id.paCloseB);
			close.Click += delegate {
				if (Pharmacy.CreatedAt == null)
				{
					MainDatabase.DeletePharmacy(Pharmacy);
				}

				transaction.Commit();
				Finish();
			};

			var save = FindViewById<Button>(Resource.Id.paSaveB);
			save.Click += delegate {
				Pharmacy.CreatedAt = DateTimeOffset.Now;
				Pharmacy.Brand = FindViewById<EditText>(Resource.Id.paBrandET).Text;
				Pharmacy.NumberName = FindViewById<EditText>(Resource.Id.paNumberNameET).Text;
				Pharmacy.LegalName = FindViewById<EditText>(Resource.Id.paLegalNameET).Text;
				Pharmacy.Address = FindViewById<AutoCompleteTextView>(Resource.Id.paAddressACTV).Text;
				//pharmacy.Subway = FindViewById<AutoCompleteTextView>(Resource.Id.paSubwayACTV).Text;
				//pharmacy.Region = FindViewById<AutoCompleteTextView>(Resource.Id.paRegionACTV).Text;
				Pharmacy.Phone = FindViewById<EditText>(Resource.Id.paPhoneET).Text;
				//pharmacy.Place = FindViewById<AutoCompleteTextView>(Resource.Id.paPlaceACTV).Text;
				//pharmacy.CategoryByNet = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryByNetACTV).Text;
				//pharmacy.CategoryBySell = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryBySellACTV).Text;
				Pharmacy.TurnOver = Helper.ToInt(FindViewById<EditText>(Resource.Id.paTurnOverET).Text);
				Pharmacy.Comment = FindViewById<EditText>(Resource.Id.paCommentET).Text;

				transaction.Commit();

				var sync = new SyncItem()
				{
					Path = @"Pharmacy",
					ObjectUUID = Pharmacy.UUID,
					JSON = JsonConvert.SerializeObject(Pharmacy)
				};

				MainDatabase.AddToQueue(sync);

				StartService(new Intent("com.xamarin.SyncService"));

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

