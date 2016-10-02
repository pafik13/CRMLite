using System;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Views.InputMethods;

using CRMLite.Entities;
using CRMLite.Adapters;
using CRMLite.Dialogs;
using Android.Locations;
using HockeyApp.Android;
using HockeyApp.Android.Utils;

[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]

namespace CRMLite
{
	[Activity(Label = "Main")]
	public class MainActivity : Activity
	{
		public const string C_MAIN_PREFS = @"C_MAIN_PREFS";
		public const string C_SAVED_PHARMACY_UUID = @"C_SAVED_PHARMACY_UUID";
		public const int C_ITEMS_IN_RESULT = 10;
		public const string C_DUMMY = @"C_DUMMY";

		IList<Pharmacy> Pharmacies;
		ListView PharmacyTable;
		TextView FilterContent;
		TextView AttendanceCount;
		EditText SearchInput;
		ListView SearchTable;
		Dictionary<string, SearchItem> SearchItems;
		ImageView Filter;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Register the crash manager before Initializing the trace writer
			CrashManager.Register(this, Secret.HockeyappAppId);

			//Register to with the Update Manager
			UpdateManager.Register(this, Secret.HockeyappAppId);

			HockeyLog.LogLevel = 3;

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			PharmacyTable = FindViewById<ListView>(Resource.Id.maPharmacyTable);
			View header = LayoutInflater.Inflate(Resource.Layout.PharmacyTableHeader, PharmacyTable, false);
			PharmacyTable.AddHeaderView(header);
			PharmacyTable.Clickable = true;
			PharmacyTable.ItemClick += OnListItemClick;

			var profile = FindViewById<ImageView>(Resource.Id.maProfile);
			profile.Click += (sender, e) => {
				StartActivity(new Intent(this, typeof(ProfileActivity)));
			};

			var add = FindViewById<ImageView>(Resource.Id.maAdd);
			add.Click += (sender, e) => {
				StartActivity(new Intent(this, typeof(PharmacyActivity)));
			};

			var sync = FindViewById<ImageView>(Resource.Id.maSync);
			sync.Click += (sender, e) => {
				StartActivity(new Intent(this, typeof(SyncActivity)));
			};
			sync.LongClick += (sender, e) => {
				StartActivity(new Intent(this, typeof(LoadDataActivity)));
			};

			var lib = FindViewById<ImageView>(Resource.Id.maLibrary);
			lib.Click += (sender, e) => {
				StartActivity(new Intent(this, typeof(LibraryActivity)));
			};
			lib.LongClick += (sender, e) => {
				StartActivity(new Intent(this, typeof(TestDataActivity)));
			};

			var route = FindViewById<ImageView>(Resource.Id.maRoute);
			route.Click += (sender, e) => {
				StartActivity(new Intent(this, typeof(RouteActivity)));
			};

			var searchView = FindViewById<RelativeLayout>(Resource.Id.maSearchRL);
			searchView.Click += (sender, e) => {
				if (CurrentFocus != null) {
					var imm = (InputMethodManager)GetSystemService(InputMethodService);
					imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.None);
				}

				searchView.Visibility = ViewStates.Gone;
			};
			var search = FindViewById<ImageView>(Resource.Id.maSearch);
			search.Click += (sender, e) => {
				searchView.Visibility = ViewStates.Visible;
			};

			var searchSettings = FindViewById<ImageView>(Resource.Id.maSearchSettingsIV);
			searchSettings.Click += (sender, e) => {
				Toast.MakeText(this, @"maSearchSettingsIV Clicked", ToastLength.Short).Show();
			};

			SearchInput = FindViewById<EditText>(Resource.Id.maSearchInput);
			SearchInput.AfterTextChanged += (sender, e) => {
				Search(e.Editable.ToString());
			};

			SearchTable = FindViewById<ListView>(Resource.Id.maSearchTable);
			SearchTable.ItemClick += (sender, e) => {
				if (sender is ListView) {
					var lv = ((ListView)sender);

					var cb = e.View.FindViewById<CheckBox>(Resource.Id.sriIsCheckedCB);
					cb.Checked = lv.IsItemChecked(e.Position);

					//e.View.Selected = lv.IsItemChecked(e.Position);
					//lv.SetItemChecked(e.Position, true);
					if (lv.Adapter is SearchResultAdapter) {
						var adapter = (SearchResultAdapter)lv.Adapter;

						var newList = new List<Pharmacy>();
						var sparseArray = lv.CheckedItemPositions;
						for (var i = 0; i < sparseArray.Size(); i++) {
							if (sparseArray.ValueAt(i)) {
								newList.Add(MainDatabase.GetEntity<Pharmacy>(adapter[sparseArray.KeyAt(i)].UUID));
							}
						}

						RecreateAdapter(newList);
					}
				}
			};
			SearchTable.ItemSelected += (sender, e) => {
				if (sender is ListView) {
					var lv = ((ListView)sender);
					if (lv.Adapter is SearchResultAdapter) {
						var adapter = (SearchResultAdapter)lv.Adapter;
						var obj = adapter[(int)e.Id];
					}
				}				
			};

			Filter = FindViewById<ImageView>(Resource.Id.maFilter);
			Filter.Click += Filter_Click;

			Filter.LongClick += (sender, e) => {
				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(FilterDialog.TAG);
				if (prev != null) {
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var filterDialog = new FilterDialog();
				filterDialog.Show(fragmentTransaction, FilterDialog.TAG);
				filterDialog.AfterSaved += (caller, arguments) => {
					GetSharedPreferences(FilterDialog.C_FILTER_PREFS, FileCreationMode.Private)
						.Edit()
						.PutBoolean(@"IS_ON", true)
						.Commit();

					RecreateAdapter();
					//Filter_Click(sender, e);
				};
			};

			var message = FindViewById<ImageView>(Resource.Id.maMessage);
			message.Click += (sender, e) => {
				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(MessageDialog.TAG);
				if (prev != null) {
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var messageDialog = new MessageDialog();
				messageDialog.Show(fragmentTransaction, MessageDialog.TAG);
				messageDialog.AfterSaved += (caller, arguments) => {
					Toast.MakeText(this, @"Message saved", ToastLength.Short).Show();
				};
			};

			message.LongClick += (sender, e) => {
				StartActivity(new Intent(this, typeof(MessageActivity)));
			};

			FilterContent = FindViewById<TextView>(Resource.Id.maFilterTV);
			AttendanceCount = FindViewById<TextView>(Resource.Id.maAttendanceCountTV);


			//LoginManager.Register(this, Secret.HockeyappAppId, LoginManager.LoginModeEmailOnly);
			//LoginManager.VerifyLogin(this, Intent);
			UpdateManager.Register(this, Secret.HockeyappAppId);
		}

		void Filter_Click(object sender, EventArgs e)
		{
			if (sender is ImageView) {
				var img = sender as ImageView;
				var prefs = GetSharedPreferences(FilterDialog.C_FILTER_PREFS, FileCreationMode.Private);
				var isFilterOn = prefs.GetBoolean(@"IS_ON", false);
				isFilterOn = !isFilterOn;
				prefs.Edit().PutBoolean(@"IS_ON", isFilterOn).Commit();

				RecreateAdapter();
			}
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			if (e.Position > 0) 
			{
				var showPharmacy = new Intent(this, typeof(PharmacyActivity));
				showPharmacy.PutExtra(@"UUID", Pharmacies[e.Position - 1].UUID);
				StartActivity(showPharmacy);
			}
		}

		void RecreateAdapter(List<Pharmacy> inputList = null)
		{
			// ветка для фильтра
			if (inputList != null) {
				if (inputList.Count > 0) {
					Pharmacies = inputList;
					PharmacyTable.Adapter = new PharmacyAdapter(this, Pharmacies);
					return;
				}
			}

			var list = MainDatabase.GetItems<Pharmacy>();
			string[] pharmaciesInRoute = null;

			var prefs = GetSharedPreferences(FilterDialog.C_FILTER_PREFS, FileCreationMode.Private);
			bool isOn = prefs.GetBoolean(@"IS_ON", false);
			string subway = prefs.GetString(@"SUBWAY", string.Empty);
			string region = prefs.GetString(@"REGION", string.Empty);
			string net = prefs.GetString(@"NET", string.Empty);
			string address = prefs.GetString(@"ADDRESS", string.Empty);

			if (isOn) {
				Filter.SetBackgroundColor(Android.Graphics.Color.LightGreen);

				var filters = new List<string>();
				if (!string.IsNullOrEmpty(subway)) {
					list = list.Where(ph => ph.Subway == subway).ToList();
					filters.Add(string.Format(@"Метро=""{0}""", MainDatabase.GetItem<Subway>(subway).name));
				}

				if (!string.IsNullOrEmpty(region)) {
					list = list.Where(ph => ph.Region == region).ToList();
					filters.Add(string.Format(@"Район=""{0}""", MainDatabase.GetItem<Region>(region).name));
				}

				if (!string.IsNullOrEmpty(net)) {
					list = list.Where(ph => ph.Net == net).ToList();
					filters.Add(string.Format(@"Аптечная сеть=""{0}""", MainDatabase.GetItem<Net>(net).name));
				}
				if (!string.IsNullOrEmpty(address)) {
					var culture = CultureInfo.GetCultureInfo("ru-RU");
					list = list.Where(ph => culture.CompareInfo.IndexOf(ph.Address, address, CompareOptions.IgnoreCase) >= 0
					                 ).ToList();
					filters.Add(string.Format(@"Адрес=""{0}""", address));
				}

				FilterContent.Text = @"ФИЛЬТР: " + string.Join("; ", filters);
				FilterContent.Visibility = ViewStates.Visible;
				FilterContent.RequestFocus();
			} else {
				FilterContent.Visibility = ViewStates.Gone;
				Filter.SetBackgroundColor(Android.Graphics.Color.Transparent);


				var orderMapState = new Dictionary<string, int> {
					{ PharmacyState.psActive.ToString("G"), 0 },
					{ PharmacyState.psReserve.ToString("G"), 1 },
					{ PharmacyState.psClose.ToString("G"), 2 },
				};

				// TODO: optimize work with route
				var now = DateTime.Now;
				var date = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, new TimeSpan(0, 0, 0));
				var routeItems = MainDatabase.GetRouteItems(date);
				pharmaciesInRoute = routeItems.Select(ri => ri.Pharmacy).ToArray();
				// wmOnlyRoute, wmRouteAndRecommendations, wmOnlyRecommendations
				switch (Helper.WorkMode) {
					case WorkMode.wmOnlyRoute:
						if (routeItems.Count == 0) {
							list = new List<Pharmacy>();
					    } else {
							list = list.Where(ph => pharmaciesInRoute.Contains(ph.UUID)).ToList();
					    }
					    break;
					case WorkMode.wmRouteAndRecommendations:
						if (routeItems.Count == 0) {
							list = list.OrderBy(ph => orderMapState[ph.State]).ThenBy(ph => ph.NextAttendanceDate).ToList();
					    } else {
							var orderMapRoute = routeItems.ToDictionary(ri => ri.Pharmacy, ri => ri.Order);  
							var routeList = list.Where(ph => pharmaciesInRoute.Contains(ph.UUID))
							                    .OrderBy(ph => orderMapRoute[ph.UUID])
							                    .ToList();

							var nonRouteList = list.Where(ph => !pharmaciesInRoute.Contains(ph.UUID))
							                       .OrderBy(ph => orderMapState[ph.State])
							                       .ThenBy(ph => ph.NextAttendanceDate)
							                       .ToList();
							list = new List<Pharmacy>();
							list.AddRange(routeList);
							list.AddRange(nonRouteList);
						}
					    break;
					case WorkMode.wmOnlyRecommendations:
						list = list.OrderBy(ph => orderMapState[ph.State]).ThenBy(ph => ph.NextAttendanceDate).ToList();
						break;
				}
				
			  //Pharmacies = list; //.Take(14).ToList();
			  //PharmacyTable.Adapter = new PharmacyAdapter(this, Pharmacies, pharmaciesInRout);
			  //return;
			}
			Filter.Invalidate();


			Pharmacies = list; //.Take(14).ToList();
			PharmacyTable.Adapter = new PharmacyAdapter(this, Pharmacies, pharmaciesInRoute);
		}

		void Search(string text)
		{
			if (string.IsNullOrEmpty(text)) {
				SearchTable.Adapter = null;
				RecreateAdapter();
				return;
			}

			//Console.WriteLine(@"Search: startmemory{0}", System.Diagnostics.Process.GetCurrentProcess().WorkingSet64);

			var w = new Stopwatch();
			w.Start();
			// 1 насыщениe
			//var searchItems = new List<SearchItem>()
			if (SearchItems == null) {
				SearchItems = new Dictionary<string, SearchItem>();
				var pharmacies = MainDatabase.GetItems<Pharmacy>();
				foreach (var item in pharmacies) {
					SearchItems.Add(
						item.UUID,
						new SearchItem(
							item.UUID,
							item.GetName(),
							string.IsNullOrEmpty(item.Subway) ? item.Subway : MainDatabase.GetItem<Subway>(item.Subway).name,
							string.IsNullOrEmpty(item.Region) ? item.Region : MainDatabase.GetItem<Region>(item.Region).name,
							item.Brand
						)
					);
				}
			}
			w.Stop();
			Console.WriteLine(@"Search: насыщение={0}", w.ElapsedMilliseconds);

			w.Restart();
			var matchFormat = @"Совпадение: {0}";
			var result = new List<SearchItem>();
			var culture = CultureInfo.GetCultureInfo("ru-RU");
			// 2 поиск
			foreach (var item in SearchItems) {
				if (culture.CompareInfo.IndexOf(item.Value.Subway, text, CompareOptions.IgnoreCase) >= 0) {
					item.Value.Match = string.Format(matchFormat, @"метро=" + item.Value.Subway);
					result.Add(item.Value);
					if (result.Count > C_ITEMS_IN_RESULT) break;
					continue;
				}

				if (culture.CompareInfo.IndexOf(item.Value.Region, text, CompareOptions.IgnoreCase) >= 0) {
					item.Value.Match = string.Format(matchFormat, @"район=" + item.Value.Region);
					result.Add(item.Value);
					if (result.Count > C_ITEMS_IN_RESULT) break;
					continue;
				}

				if (culture.CompareInfo.IndexOf(item.Value.Brand, text, CompareOptions.IgnoreCase) >= 0) {
					item.Value.Match = string.Format(matchFormat, @"бренд=" + item.Value.Brand);
					result.Add(item.Value);
					if (result.Count > C_ITEMS_IN_RESULT) break;
					continue;
				}
			}
			w.Stop();
			Console.WriteLine(@"Search: поиск={0}", w.ElapsedMilliseconds);

			w.Restart();
			// 3 показ
			SearchTable.Adapter = new SearchResultAdapter(this, result);
			//SearchTable.Adapter = new ArrayAdapter<string>(
			//	this, Android.Resource.Layout.SimpleListItemMultipleChoice, result.Select(si => si.Name).ToArray()
			//);
			w.Stop();
			Console.WriteLine(@"Search: показ={0}", w.ElapsedMilliseconds);
			//Console.WriteLine(@"Search: endmemory{0}", System.Diagnostics.Process.GetCurrentProcess().WorkingSet64);
		}

		protected override void OnResume()
		{
			base.OnResume();

			//Start Tracking usage in this activit
			Tracking.StartUsage(this);

			var shared = GetSharedPreferences(C_MAIN_PREFS, FileCreationMode.Private);

			string username = shared.GetString(SigninDialog.C_USERNAME, string.Empty);
			if (string.IsNullOrEmpty(username)) {
				Pharmacies = new List<Pharmacy>(); //.Take(14).ToList();
				PharmacyTable.Adapter = new PharmacyAdapter(this, Pharmacies);

				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(SigninDialog.TAG);
				if (prev != null) {
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var signinDialog = new SigninDialog(this);
				signinDialog.Show(fragmentTransaction, SigninDialog.TAG);
				signinDialog.SuccessSignedIn += (caller, arguments) => {
					//Toast.MakeText(this, @"SuccessSignedIn", ToastLength.Short).Show();
					RunOnUiThread(() => {
						OnResume();
						Console.WriteLine(@"DBPath = {0}", MainDatabase.DBPath);
					});
				};

				Console.WriteLine(@"username = IsNullOrEmpty");
				return;
			}

			if (!IsLocationActive()) return;

			MainDatabase.Username = username;
			Helper.Username = username;

			var email = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);

			var agentUUID = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);
			try {
				var agent = MainDatabase.GetItem<Agent>(agentUUID);
				MainDatabase.AgentUUID = agent.uuid;
				Helper.WeeksInRoute = agent.weeksInRout;
				Helper.WorkMode = agent.GetWorkMode();
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
			}


			var w = new Stopwatch();
			
			// TODO: сделать ветку автоматической синхронизации
			// w.Restart();
			// StartService(new Intent(@"SyncService"));
			// w.Stop();
			// Console.WriteLine(@"SyncService: запуск={0}", w.ElapsedMilliseconds);

		    w.Restart();
			var currentRouteItems = MainDatabase.GetRouteItems(DateTimeOffset.Now);

		    if (currentRouteItems.Count() > 0){
				var nextDate = DateTimeOffset.Now.AddDays(Helper.WeeksInRoute * 7);
				var nextRoutItems = MainDatabase.GetRouteItems(nextDate);

		      if (nextRoutItems.Count() == 0) {
		        using (var trans = MainDatabase.BeginTransaction()){
		          foreach (var oldItem in currentRouteItems){
		            var newItem = MainDatabase.Create2<RouteItem>();
						      newItem.Pharmacy = oldItem.Pharmacy;
						      newItem.Date = nextDate;
						      newItem.Order = oldItem.Order;
		          }
		          trans.Commit();
		        }
		      }
		    }
		    w.Stop();
		    Console.WriteLine(@"Route: копирование={0}", w.ElapsedMilliseconds);
    
			w.Restart();
			int count = MainDatabase.GetItems<Attendance>()
									.Count(att => att.When.LocalDateTime.Date == DateTimeOffset.Now.Date);
			AttendanceCount.Text = string.Format(@"РЕЖИМ РАБОТЫ: {0};  СЕГОДНЯ ВИЗИТОВ: {1}", Helper.GetWorkModeDesc(Helper.WorkMode), count);
			w.Stop();
			Console.WriteLine(@"OnResume: подсчет визитов={0}", w.ElapsedMilliseconds);

			var sparseArray = SearchTable.CheckedItemPositions;
			bool hasCheckedItemInSearchTable = false;
			for (var i = 0; i < sparseArray.Size(); i++) {
				if (sparseArray.ValueAt(i)) {
					hasCheckedItemInSearchTable = true;
					break;
				}
			}

			if (!hasCheckedItemInSearchTable) {
				RecreateAdapter();
			}

			var prefs = GetSharedPreferences(C_MAIN_PREFS, FileCreationMode.Private);
			var uuid = prefs.GetString(C_SAVED_PHARMACY_UUID, string.Empty);
			if (string.IsNullOrEmpty(uuid)) return;

			if (SearchItems == null) return;

			w.Start();
			var pharmacy = MainDatabase.GetEntity<Pharmacy>(uuid);
			SearchItems[uuid] = new SearchItem(
				pharmacy.UUID,
				pharmacy.GetName(),
				string.IsNullOrEmpty(pharmacy.Subway) ? pharmacy.Subway : MainDatabase.GetItem<Subway>(pharmacy.Subway).name,
				string.IsNullOrEmpty(pharmacy.Region) ? pharmacy.Region : MainDatabase.GetItem<Region>(pharmacy.Region).name,
				pharmacy.Brand
			);
			w.Stop();
			Console.WriteLine(@"Search: обновление={0}", w.ElapsedMilliseconds);
		}

		bool IsLocationActive()
		{
			/*throw new NotImplementedException();*/
			var locMgr = GetSystemService(LocationService) as LocationManager;

			if (locMgr.IsProviderEnabled(LocationManager.NetworkProvider)
			  || locMgr.IsProviderEnabled(LocationManager.GpsProvider)
			   ) {
				return true;
			}

			new AlertDialog.Builder(this)
						   .SetTitle(Resource.String.warning_caption)
						   .SetMessage(Resource.String.no_location_provider)
						   .SetCancelable(false)
						   .SetPositiveButton(Resource.String.on_button, delegate {
							   var intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
							   StartActivity(intent);
						   })
						   .Show();

			return false;
		}

		protected override void OnPause()
		{
			//Stop Tracking usage in this activity
			Tracking.StopUsage(this);
			UpdateManager.Unregister();
			base.OnPause();
		}

		protected override void OnStop()
		{
			base.OnStop();
			// TODO: сделать ветку автоматической синхронизации
			// SaveToFileAllCache;
			UpdateManager.Unregister();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			UpdateManager.Unregister();
		}
	}
}


