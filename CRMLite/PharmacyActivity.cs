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
using CRMLite.DaData;
using CRMLite.Adapters;
using CRMLite.Dialogs;

namespace CRMLite
{
	[Activity(Label = "PharmacyActivity", WindowSoftInputMode=SoftInput.StateHidden)]
	public class PharmacyActivity : Activity
	{
		Pharmacy Pharmacy;

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
		IList<Category> CategoryByNets;
		AutoCompleteTextView Category;

		SuggestClient Api;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Pharmacy);

			FindViewById<Button>(Resource.Id.paCloseB).Click += (s, e) => {
				Finish();
			};

			FindViewById<Button>(Resource.Id.paSaveB).Click += (s, e) => {
				var transaction = MainDatabase.BeginTransaction();

				Pharmacy item;
				if (Pharmacy == null) {
					item = MainDatabase.Create<Pharmacy>();
					item.CreatedAt = DateTimeOffset.Now;

					/* Contracts */
					if (!string.IsNullOrEmpty(ContractsNames.Text)) {
						var ll = ContractsNames.Parent as LinearLayout;
						var contractUUIDs = (string)ll.GetTag(Resource.String.ContractUUIDs);
						if (!string.IsNullOrEmpty(contractUUIDs)) {
							foreach (var contract in contractUUIDs.Split(';')) {
								var contractData = MainDatabase.Create<ContractData>();
								contractData.Pharmacy = item.UUID;
								contractData.Contract = contract; 
							}
						}
					}
					/* ./Contracts */
				} else {
					item = Pharmacy;

					/* Contracts */
					if (string.IsNullOrEmpty(ContractsNames.Text)) {
						var contractDatas = MainDatabase.GetPharmacyDatas<ContractData>(item.UUID);
						foreach (var contractData in contractDatas) {
							MainDatabase.DeleteEntity(transaction, contractData);;
						}
						contractDatas = null;
					} else {
						var ll = ContractsNames.Parent as LinearLayout;
						var contractUUIDs = (string)ll.GetTag(Resource.String.ContractUUIDs);
						if (!string.IsNullOrEmpty(contractUUIDs)) {
							var contracts = contractUUIDs.Split(';');
							var contractDatas = MainDatabase.GetPharmacyDatas<ContractData>(item.UUID);
							foreach (var contractData in contractDatas) {
								MainDatabase.DeleteEntity(transaction, contractData); ;
							}
							contractDatas = null;
							foreach (var contract in contractUUIDs.Split(';')) {
								var contractData = MainDatabase.Create<ContractData>();
								contractData.Pharmacy = item.UUID;
								contractData.Contract = contract;
							}
						}
					}
					/* ./Contracts */
				}

				item.UpdatedAt = DateTimeOffset.Now;
				item.SetState((PharmacyState)State.SelectedItemPosition);
				item.Brand = FindViewById<EditText>(Resource.Id.paBrandET).Text;
				item.NumberName = FindViewById<EditText>(Resource.Id.paNumberNameET).Text;
				item.LegalName = FindViewById<EditText>(Resource.Id.paLegalNameET).Text;

				if (string.IsNullOrEmpty(NetName.Text)) {
					item.Net = string.Empty;
				} else {
					item.Net = NetUUID;
				}

				item.Address = FindViewById<AutoCompleteTextView>(Resource.Id.paAddressACTV).Text;

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

				item.Phone = FindViewById<EditText>(Resource.Id.paPhoneET).Text;

				if (string.IsNullOrEmpty(Place.Text)) {
					item.Place = string.Empty;
				} else {
					var placeUUID = (string)Place.GetTag(Resource.String.PlaceUUID);
					if (!string.IsNullOrEmpty(placeUUID)) {
						item.Place = placeUUID;
					}
				}

				if (string.IsNullOrEmpty(Category.Text)) {
					item.Category = string.Empty;
				} else {
					var categoryUUID = (string)Category.GetTag(Resource.String.CategoryUUID);
					if (!string.IsNullOrEmpty(categoryUUID)) {
						item.Category = categoryUUID;
					}
				}
				item.TurnOver = Helper.ToInt(FindViewById<EditText>(Resource.Id.paTurnOverET).Text);
				item.Comment = FindViewById<EditText>(Resource.Id.paCommentET).Text;

				if (!item.IsManaged) MainDatabase.SaveEntity(transaction, item);

				transaction.Commit();

				//var sync = new SyncItem() {
				//	Path = @"Pharmacy",
				//	ObjectUUID = Pharmacy.UUID,
				//	JSON = JsonConvert.SerializeObject(Pharmacy)
				//};

				//MainDatabase.AddToQueue(sync);

				//StartService(new Intent("com.xamarin.SyncService"));
				GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private)
					.Edit()
					.PutString(MainActivity.C_SAVED_PHARMACY_UUID, item.UUID)
					.Commit();

				Finish();
			};

			Api = new SuggestClient(Secret.DadataApiToken, Secret.DadataApiURL);


			#region State
			State = FindViewById<Spinner>(Resource.Id.paStateS);
			States = MainDatabase.GetStates();
			var stateAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, States.ToArray());
			stateAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			State.Adapter = stateAdapter;
			#endregion

			#region Net
			Nets = MainDatabase.GetNets();
			NetName = FindViewById<AutoCompleteTextView>(Resource.Id.paNetACTV);
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
			ContractsChoice.Click += ContractsChoice_Click;

			Address = FindViewById<AutoCompleteTextView>(Resource.Id.paAddressACTV);

			Subway = FindViewById<AutoCompleteTextView>(Resource.Id.paSubwayACTV);

			Region = FindViewById<AutoCompleteTextView>(Resource.Id.paRegionACTV);

			Place = FindViewById<AutoCompleteTextView>(Resource.Id.paPlaceACTV);

			Category = FindViewById<AutoCompleteTextView>(Resource.Id.paCategoryACTV);

			var pharmacyUUID = Intent.GetStringExtra("UUID");
			if (string.IsNullOrEmpty(pharmacyUUID))
			{
				var shared = GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private);
				var agentUUID = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);
				try {
					var agent = MainDatabase.GetItem<Agent>(agentUUID);
					Address.Text = agent.city;
				} catch (Exception ex) {
					Console.WriteLine(ex.Message);
				}

				FindViewById<TextView>(Resource.Id.paInfoTV).Text = @"ДОБАВЛЕНИЕ НОВОЙ АПТЕКИ";
				FindViewById<TableRow>(Resource.Id.paRowLastAttendance).Visibility = ViewStates.Gone;
				FindViewById<TableRow>(Resource.Id.paRowNextAttendanceDate).Visibility = ViewStates.Gone;

				InitViews();
				return;
			}

			Pharmacy = MainDatabase.GetEntity<Pharmacy>(pharmacyUUID);

			FindViewById<TextView>(Resource.Id.paInfoTV).Text = "АПТЕКА : " + Pharmacy.GetName();

			FindViewById<TextView>(Resource.Id.paUUIDTV).Text = Pharmacy.UUID;

			State.SetSelection((int)Pharmacy.GetState());
			FindViewById<EditText>(Resource.Id.paBrandET).Text = Pharmacy.Brand;
			FindViewById<EditText>(Resource.Id.paNumberNameET).Text = Pharmacy.NumberName;
			FindViewById<EditText>(Resource.Id.paLegalNameET).Text = Pharmacy.LegalName;

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

			FindViewById<EditText>(Resource.Id.paPhoneET).Text = Pharmacy.Phone;

			Place.Text = string.IsNullOrEmpty(Pharmacy.Place) ?
				string.Empty : MainDatabase.GetItem<Place>(Pharmacy.Place).name;

			Category.Text = string.IsNullOrEmpty(Pharmacy.Category) ?
				string.Empty : MainDatabase.GetItem<Category>(Pharmacy.Category).name;

			FindViewById<EditText>(Resource.Id.paTurnOverET).Text = Pharmacy.TurnOver.HasValue ?
				Pharmacy.TurnOver.Value.ToString() : string.Empty;

			FindViewById<TextView>(Resource.Id.paLastAttendanceTV).Text = Pharmacy.LastAttendanceDate.HasValue ?
				Pharmacy.LastAttendanceDate.Value.ToString(@"dd.MM.yyyy") : @"<нет визита>";

			FindViewById<TextView>(Resource.Id.paNextAttendanceDateTV).Text = Pharmacy.NextAttendanceDate.HasValue ?
				Pharmacy.NextAttendanceDate.Value.ToString(@"dd.MM.yyyy") : DateTimeOffset.Now.ToString(@"dd.MM.yyyy");

			FindViewById<EditText>(Resource.Id.paCommentET).Text = Pharmacy.Comment;


			InitViews();
		}

		void InitViews()
		{
			#region Address
			Address.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) => {
				if (Address.IsPerformingCompletion) return;

				if (Address.Text.Contains(" ")) {
					var suggestions = new List<SuggestAddressResponse.Suggestions>();
					try {
						var response = Api.QueryAddress(Address.Text);
						suggestions = response.suggestionss;
					} catch (Exception ex) {
						System.Diagnostics.Debug.WriteLine(ex.Message);
					}
					Address.Adapter = new AddressSuggestionAdapter(this, suggestions);

					if (Address.IsShown) {
						Address.DismissDropDown();
					}
					Address.ShowDropDown();
				}
			};
			Address.ItemClick += (sender, e) => {
				var item = (((AutoCompleteTextView)sender).Adapter as AddressSuggestionAdapter)[e.Position];
				((AutoCompleteTextView)sender).SetTag(Resource.String.fias_id, item.data.fias_id);
				((AutoCompleteTextView)sender).SetTag(Resource.String.qc_geo, item.data.qc_geo);
				((AutoCompleteTextView)sender).SetTag(Resource.String.geo_lat, item.data.geo_lat);
				((AutoCompleteTextView)sender).SetTag(Resource.String.geo_lon, item.data.geo_lon);
			};
			#endregion

			#region Subway
			Subways = MainDatabase.GetItems<Subway>();
			Subway.Adapter = new ArrayAdapter<string>(
				this, Android.Resource.Layout.SimpleDropDownItem1Line, Subways.Select(s => s.name).ToArray()
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
				this, Android.Resource.Layout.SimpleDropDownItem1Line, Regions.Select(r => r.name).ToArray()
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
				this, Android.Resource.Layout.SimpleDropDownItem1Line, Places.Select(x => x.name).ToArray()
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
    
			#region Category
			CategoryByNets = MainDatabase.GetCategories("net");
			Category.Adapter = new ArrayAdapter<string>(
				this, Android.Resource.Layout.SimpleDropDownItem1Line, CategoryByNets.Select(c => c.name).ToArray()
			);
			Category.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				if (sender is AutoCompleteTextView) {
					var text = ((AutoCompleteTextView)sender).Text;
					var category = CategoryByNets.SingleOrDefault(c => c.name.Equals(text));
					if (category != null) {
						((AutoCompleteTextView)sender).SetTag(Resource.String.CategoryUUID, category.uuid);
					}
				}
			};
			#endregion
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

				new AlertDialog.Builder(this)
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
										(caller as Dialog).Dispose();
								   }
								)
				               .SetNegativeButton(@"Отмена", (caller, arguments) => { (caller as Dialog).Dispose(); })
				               .Show();

				//new AlertDialog.Builder(this)
				//			   .SetTitle("Контракты")
				//			   .SetCancelable(true)
				//			   .SetItems(Contracts.Select(item => item.name).ToArray(), (caller, arguments) => {
				//				   ContractsNames.Text = Contracts[arguments.Which].name;
				//				   ContractsNames.SetTag(Resource.String.ContractUUID, Contracts[arguments.Which].uuid);
				//			   })
				//			   .Show();
			}
		}
	}
}

