using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Android.OS;
using Android.Views;
using Android.Widget;

using Android.Support.V4.App;
using CRMLite.Entities;
using CRMLite.DaData;
using Realms;
using CRMLite.Adapters;
using Android.Content;
using CRMLite.Dialogs;

namespace CRMLite
{
	public class PharmacyFragment : Fragment, IAttendanceControl
	{
		Stopwatch Chrono;

		public const string C_PHARMACY_UUID = @"C_PHARMACY_UUID";

		Pharmacy Pharmacy;
		Agent Agent;
		Spinner State;
		IList<string> States;

		List<Net> Nets;
		string NetUUID;
		AutoCompleteTextView NetName;

		IList<Contract> Contracts;
		IList<ContractData> ContractDatas;
		AutoCompleteTextView ContractsNames;
		Button ContractsChoice;

		AutoCompleteTextView Address;

		IList<Subway> Subways;
		AutoCompleteTextView Subway;
		IList<Region> Regions;
		AutoCompleteTextView Region;
		IList<Place> Places;
		AutoCompleteTextView Place;
		List<Category> Categories;
		Spinner Category;

		//SuggestClient Api;

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
			Chrono = new Stopwatch();
			Chrono.Start();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);

			View view = inflater.Inflate(Resource.Layout.PharmacyFragment, container, false);
			//Api = new SuggestClient(Secret.DadataApiToken, Secret.DadataApiURL);


			var pharmacyUUID = Arguments.GetString(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return view;

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);

			var shared = Activity.GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private);

			var agentUUID = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);
			try {
				Agent = MainDatabase.GetItem<Agent>(agentUUID);
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
				Agent = null;
			}

			#region State
			State = view.FindViewById<Spinner>(Resource.Id.pfStateS);
			States = MainDatabase.GetStates();
			var stateAdapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerItem, States.ToArray());
			stateAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			State.Adapter = stateAdapter;
			#endregion

			#region Net
			Nets = MainDatabase.GetNets();
			NetName = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfNetACTV);
			var netChoiceButton = view.FindViewById<Button>(Resource.Id.pfNetB);
			netChoiceButton.Click += (object sender, EventArgs e) => {
				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Аптечная сеть")
						   .SetCancelable(true)
						   .SetItems(Nets.Select(item => item.name).ToArray(), (caller, arguments) => {
							   SetNet(arguments.Which);
							   //Toast.MakeText(this, @"Selected " + arguments.Which, ToastLength.Short).Show();
						   })
						   .Show();
			};
			#endregion

			ContractsNames = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfContractsACTV);
			ContractsChoice = view.FindViewById<Button>(Resource.Id.pfContractsB);
			ContractsChoice.Click += ContractsChoice_Click;

			Address = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfAddressACTV);
			Address.SetTag(Resource.String.IsChanged, false);

			Subway = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfSubwayACTV);

			Region = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfRegionACTV);

			Place = view.FindViewById<AutoCompleteTextView>(Resource.Id.pfPlaceACTV);

			//Category = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryACTV);

			#region Category
			Category = view.FindViewById<Spinner>(Resource.Id.pfCategoryS);
			Categories = new List<Category>();
			Categories.Add(new Category { name = @"Выберите категорию!", uuid = Guid.Empty.ToString() });
			Categories.AddRange(MainDatabase.GetItems<Category>());
			var categoryAdapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerItem, Categories.Select(cat => cat.name).ToArray());
			categoryAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			Category.Adapter = categoryAdapter;
			#endregion

			view.FindViewById<TextView>(Resource.Id.pfUUIDTV).Text = Pharmacy.UUID;

			State.SetSelection((int)Pharmacy.GetState());
			view.FindViewById<EditText>(Resource.Id.pfBrandET).Text = Pharmacy.Brand;
			view.FindViewById<EditText>(Resource.Id.pfNumberNameET).Text = Pharmacy.NumberName;
			view.FindViewById<EditText>(Resource.Id.pfLegalNameET).Text = Pharmacy.LegalName;

			//NetName.Text = string.IsNullOrEmpty(Pharmacy.Net) ?
			//	string.Empty : MainDatabase.GetNet(Pharmacy.Net).name;
			//NetUUID = Pharmacy.Net;

			if (!string.IsNullOrEmpty(Pharmacy.Net)) {
				SetNet(Nets.FindIndex(net => string.Compare(net.uuid, Pharmacy.Net) == 0));
			}

			ContractDatas = MainDatabase.GetPharmacyDatas<ContractData>(Pharmacy.UUID);
			if (ContractDatas.Count > 0) {
				ContractsNames.Text = string.Join(", ", ContractDatas.Select(cd => MainDatabase.GetItem<Contract>(cd.Contract).name).ToArray());
				var ll = ContractsNames.Parent as LinearLayout;
				ll.SetTag(Resource.String.ContractUUIDs,
						  string.Join(@";", ContractDatas.Select(cd => cd.Contract).ToArray())
						 );
			}
			Address.Text = Pharmacy.Address;

			Subway.Text = string.IsNullOrEmpty(Pharmacy.Subway) ?
				string.Empty : MainDatabase.GetItem<Subway>(Pharmacy.Subway).name;

			Region.Text = string.IsNullOrEmpty(Pharmacy.Region) ?
				string.Empty : MainDatabase.GetItem<Region>(Pharmacy.Region).name;

			view.FindViewById<EditText>(Resource.Id.pfPhoneET).Text = Pharmacy.Phone;

			Place.Text = string.IsNullOrEmpty(Pharmacy.Place) ?
				string.Empty : MainDatabase.GetItem<Place>(Pharmacy.Place).name;

			//Category.Text = string.IsNullOrEmpty(Pharmacy.Category) ?
			//	string.Empty : MainDatabase.GetItem<Category>(Pharmacy.Category).name
			if (!string.IsNullOrEmpty(Pharmacy.Category)) {
				Category.SetSelection(Categories.FindIndex(cat => string.Compare(cat.uuid, Pharmacy.Category) == 0));
			}

			view.FindViewById<EditText>(Resource.Id.pfTurnOverET).Text = Pharmacy.TurnOver.HasValue ?
				Pharmacy.TurnOver.Value.ToString() : string.Empty;

			view.FindViewById<TextView>(Resource.Id.pfLastAttendanceTV).Text = Pharmacy.LastAttendanceDate.HasValue ?
				Pharmacy.LastAttendanceDate.Value.ToString(@"dd.MM.yyyy") : @"<нет визита>";

			view.FindViewById<TextView>(Resource.Id.pfNextAttendanceDateTV).Text = Pharmacy.NextAttendanceDate.HasValue ?
				Pharmacy.NextAttendanceDate.Value.ToString(@"dd.MM.yyyy") : DateTimeOffset.Now.ToString(@"dd.MM.yyyy");

			view.FindViewById<EditText>(Resource.Id.pfCommentET).Text = Pharmacy.Comment;


			InitViews();

			return view;
		}

		void InitViews()
		{
			#region Address
			//Address.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) => {
			//	if (Address.IsPerformingCompletion) return;

			//	if (Address.Text.Contains(" ")) {
			//		var suggestions = new List<SuggestAddressResponse.Suggestions>();
			//		try {
			//			var response = Api.QueryAddress(Address.Text);
			//			suggestions = response.suggestionss;
			//		} catch (Exception ex) {
			//			System.Diagnostics.Debug.WriteLine(ex.Message);
			//		}
			//		Address.Adapter = new AddressSuggestionAdapter(Activity, suggestions);

			//		if (Address.IsShown) {
			//			Address.DismissDropDown();
			//		}
			//		Address.ShowDropDown();
			//	}
			//};
			//Address.ItemClick += (sender, e) => {
			//	var item = (((AutoCompleteTextView)sender).Adapter as AddressSuggestionAdapter)[e.Position];
			//	((AutoCompleteTextView)sender).SetTag(Resource.String.IsChanged, true);
			//	((AutoCompleteTextView)sender).SetTag(Resource.String.fias_id, item.data.fias_id);
			//	((AutoCompleteTextView)sender).SetTag(Resource.String.qc_geo, item.data.qc_geo);
			//	((AutoCompleteTextView)sender).SetTag(Resource.String.geo_lat, item.data.geo_lat);
			//	((AutoCompleteTextView)sender).SetTag(Resource.String.geo_lon, item.data.geo_lon);
			//};
			#endregion

			#region Subway
			Subways = MainDatabase.GetItems<Subway>();
			Subway.Adapter = new ArrayAdapter<string>(
				Activity, Android.Resource.Layout.SimpleDropDownItem1Line, Subways.Select(s => s.name).ToArray()
			);
			Subway.ItemClick += (sender, e) => {
				if (sender is AutoCompleteTextView) {
					var text = ((AutoCompleteTextView)sender).Text;
					var subway = Subways.SingleOrDefault(s => s.name.Equals(text));
					if (subway != null) {
						((AutoCompleteTextView)sender).SetTag(Resource.String.SubwayUUID, subway.uuid);
					}
				}
			};
			#endregion

			#region Region
			Regions = MainDatabase.GetItems<Region>();
			Region.Adapter = new ArrayAdapter<string>(
				Activity, Android.Resource.Layout.SimpleDropDownItem1Line, Regions.Select(r => r.name).ToArray()
			);
			Region.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				if (sender is AutoCompleteTextView) {
					var text = ((AutoCompleteTextView)sender).Text;
					var region = Regions.SingleOrDefault(r => r.name.Equals(text));
					if (region != null) {
						((AutoCompleteTextView)sender).SetTag(Resource.String.RegionUUID, region.uuid);
					}
				}
			};
			#endregion

			#region Place
			Places = MainDatabase.GetItems<Place>();
			Place.Adapter = new ArrayAdapter<string>(
				Activity, Android.Resource.Layout.SimpleDropDownItem1Line, Places.Select(x => x.name).ToArray()
			);
			Place.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				if (sender is AutoCompleteTextView) {
					var text = ((AutoCompleteTextView)sender).Text;
					var place = Places.SingleOrDefault(p => p.name.Equals(text));
					if (place != null) {
						((AutoCompleteTextView)sender).SetTag(Resource.String.PlaceUUID, place.uuid);
					}
				}
			};
			#endregion

			//#region Category
			//Categories = MainDatabase.GetItems<Category>();
			//Category.Adapter = new ArrayAdapter<string>(
			//	this, Android.Resource.Layout.SimpleDropDownItem1Line, Categories.Select(c => c.name).ToArray()
			//);
			//Category.FocusChange += (sender, e) => {
			//	if (Categories.Count > 0 && Categories.Count < 10) {
			//		if (sender is AutoCompleteTextView) {
			//			var actv = (AutoCompleteTextView)sender;
			//			if (actv.HasFocus) {
			//				actv.ShowDropDown();
			//			}
			//		}
			//	}
			//};
			//Category.AfterTextChanged += (sender, e) => {
			//	if (Categories.Count > 0 && Categories.Count < 10) {
			//		if (sender is AutoCompleteTextView) {
			//			var actv = (AutoCompleteTextView)sender;
			//			if (actv.HasFocus) {
			//				actv.ShowDropDown();
			//			}
			//		}
			//	}
			//};
			//Category.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
			//	if (sender is AutoCompleteTextView) {
			//		var text = ((AutoCompleteTextView)sender).Text;
			//		var category = Categories.SingleOrDefault(c => c.name.Equals(text));
			//		if (category != null) {
			//			((AutoCompleteTextView)sender).SetTag(Resource.String.CategoryUUID, category.uuid);
			//		}
			//	}
			//};
			//#endregion
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

					//ContractsChoice.Click -= ContractsChoice_Click;
				}
			}
		}

		void ContractsChoice_Click(object sender, EventArgs e)
		{
			if (sender is Button) {
				var ll = ((Button)sender).Parent as LinearLayout;

				var contractUUIDs = (string)ll.GetTag(Resource.String.ContractUUIDs);
				var cacheContracts = string.IsNullOrEmpty(contractUUIDs) ? new List<string>() : contractUUIDs.Split(';').ToList();

				bool[] checkedItems = new bool[Contracts.Count];
				if (cacheContracts.Count > 0) {
					for (int i = 0; i < Contracts.Count; i++) {
						checkedItems[i] = cacheContracts.Contains(Contracts[i].uuid);
					}
				}

				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите контракты:")
						   .SetCancelable(false)
						   .SetMultiChoiceItems(
							   Contracts.Select(item => item.name).ToArray(),
							   checkedItems,
							   (caller, arguments) => {
								   if (arguments.IsChecked) {
									   cacheContracts.Add(Contracts[arguments.Which].uuid);
								   } else {
									   cacheContracts.Remove(Contracts[arguments.Which].uuid);
								   }
							   }
							  )
						   .SetPositiveButton(
							   @"Сохранить",
							   (caller, arguments) => {
								   ll.SetTag(Resource.String.ContractUUIDs, string.Join(@";", cacheContracts));
								   if (cacheContracts.Count > 0) {
									   ContractsNames.Text = string.Join(@", ",
																		 Contracts.Where(c => cacheContracts.Contains(c.uuid))
																	   .Select(c => c.name).ToArray()
															  );
								   } else {
									   ContractsNames.Text = string.Empty;
								   }
									(caller as Android.App.Dialog).Dispose();
							   }
							)
						   .SetNegativeButton(@"Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						   .Show();
			}
		}

		public override void OnPause()
		{
			base.OnPause();
			var transaction = MainDatabase.BeginTransaction();

			Pharmacy item;

			item = Pharmacy;

			/* Contracts */
			if (string.IsNullOrEmpty(ContractsNames.Text)) {
				var contractDatas = MainDatabase.GetPharmacyDatas<ContractData>(item.UUID);
				foreach (var contractData in contractDatas) {
					MainDatabase.DeleteEntity(transaction, contractData);
				}
				contractDatas = null;
			} else {
				var ll = ContractsNames.Parent as LinearLayout;
				var contractUUIDs = (string)ll.GetTag(Resource.String.ContractUUIDs);
				if (!string.IsNullOrEmpty(contractUUIDs)) {
					var contracts = contractUUIDs.Split(';');
					var contractDatas = MainDatabase.GetPharmacyDatas<ContractData>(item.UUID);
					foreach (var contractData in contractDatas) {
						MainDatabase.DeleteEntity(transaction, contractData);
					}
					contractDatas = null;
					foreach (var contract in contractUUIDs.Split(';')) {
						var contractData = MainDatabase.Create2<ContractData>();
						contractData.Pharmacy = item.UUID;
						contractData.Contract = contract;
					}
				}
			}
			/* ./Contracts */

			item.UpdatedAt = DateTimeOffset.Now;
			item.IsSynced = false;
			item.SetState((PharmacyState)State.SelectedItemPosition);
			item.Brand = View.FindViewById<EditText>(Resource.Id.pfBrandET).Text;
			item.NumberName = View.FindViewById<EditText>(Resource.Id.pfNumberNameET).Text;
			item.LegalName = View.FindViewById<EditText>(Resource.Id.pfLegalNameET).Text;

			if (string.IsNullOrEmpty(NetName.Text)) {
				item.Net = string.Empty;
			} else {
				item.Net = NetUUID;
			}

			var address = View.FindViewById<AutoCompleteTextView>(Resource.Id.pfAddressACTV);
			item.Address = address.Text;

			bool isChanged = (bool)address.GetTag(Resource.String.IsChanged);
			if (isChanged) {
				item.AddressFiasId = (string)address.GetTag(Resource.String.fias_id);
				item.AddressQCGeo = (string)address.GetTag(Resource.String.qc_geo);
				item.AddressGeoLat = (string)address.GetTag(Resource.String.geo_lat);
				item.AddressGeoLon = (string)address.GetTag(Resource.String.geo_lon);
			}

			if (string.IsNullOrEmpty(Subway.Text)) {
				item.Subway = string.Empty;
			} else {
				var subwayUUID = (string)Subway.GetTag(Resource.String.SubwayUUID);
				if (!string.IsNullOrEmpty(subwayUUID)) {
					item.Subway = subwayUUID;
				}
			}

			if (string.IsNullOrEmpty(Region.Text)) {
				item.Region = string.Empty;
			} else {
				var regionUUID = (string)Region.GetTag(Resource.String.RegionUUID);
				if (!string.IsNullOrEmpty(regionUUID)) {
					item.Region = regionUUID;
				}
			}

			item.Phone = View.FindViewById<EditText>(Resource.Id.pfPhoneET).Text;

			if (string.IsNullOrEmpty(Place.Text)) {
				item.Place = string.Empty;
			} else {
				var placeUUID = (string)Place.GetTag(Resource.String.PlaceUUID);
				if (!string.IsNullOrEmpty(placeUUID)) {
					item.Place = placeUUID;
				}
			}

			if (Category.SelectedItemPosition > 0) {
				item.Category = Categories[Category.SelectedItemPosition].uuid;
			} else {
				item.Category = string.Empty;
			}
			//if (string.IsNullOrEmpty(Category.Text)) {
			//	item.Category = string.Empty;
			//} else {
			//	var categoryUUID = (string)Category.GetTag(Resource.String.CategoryUUID);
			//	if (!string.IsNullOrEmpty(categoryUUID)) {
			//		item.Category = categoryUUID;
			//	}
			//}
			item.TurnOver = Helper.ToInt(View.FindViewById<EditText>(Resource.Id.pfTurnOverET).Text);
			item.Comment = View.FindViewById<EditText>(Resource.Id.pfCommentET).Text;

			if (!item.IsManaged) MainDatabase.SaveEntity(transaction, item);

			transaction.Commit();

			//var sync = new SyncItem() {
			//	Path = @"Pharmacy",
			//	ObjectUUID = Pharmacy.UUID,
			//	JSON = JsonConvert.SerializeObject(Pharmacy)
			//};

			//MainDatabase.AddToQueue(sync);

			//StartService(new Intent("com.xamarin.SyncService"));
			Activity.GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private)
				.Edit()
				.PutString(MainActivity.C_SAVED_PHARMACY_UUID, item.UUID)
				.Commit();
		}

		public override void OnResume()
		{
			base.OnResume();

			string debug = string.Concat(AttendanceActivity.C_TAG_FOR_DEBUG, "-", "PharmacyFragment", ":", Chrono.ElapsedMilliseconds);
			System.Diagnostics.Debug.WriteLine(debug);
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
			pharmacy.NextAttendanceDate = Agent == null ? current.When.AddDays(14) : current.When.AddDays(Agent.weeksInRout * 7);
		}
	}
}

