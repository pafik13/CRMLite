using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using SDiag = System.Diagnostics;

using Android.OS;
using Android.Net;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.Views.InputMethods;

using HockeyApp.Android;
using HockeyApp.Android.Utils;

using CRMLite.Entities;
using CRMLite.Adapters;
using CRMLite.Dialogs;
using CRMLite.Services;

[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]

namespace CRMLite
{
	public class MyCrashManagerListener : CrashManagerListener
	{
		public WeakReference<Context> ContextHolder { get; set; }

		public override string Description {
			get {
				var result = string.Empty;
				var androidId = string.Empty;
				var agentUUID = string.Empty;
				Context context;
				if ((ContextHolder != null) && ContextHolder.TryGetTarget(out context)) {
					androidId = Helper.GetAndroidId(context);

					agentUUID = context.GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private)
									   .GetString(SigninDialog.C_AGENT_UUID, string.Empty);
				} else {
					androidId = Helper.AndroidId;

					agentUUID = MainDatabase.AgentUUID;
				}

				result = string.Concat("AndroidId: ", androidId, System.Environment.NewLine, "AgentUUID: ", agentUUID);

				return result;
			}
		}

		public override string UserID {
			get {
				return Helper.AndroidId;
			}
		}

		public override string Contact {
			get {
				return MainDatabase.AgentUUID;
			}
		}
	}

	[Activity(Label = "Main", ScreenOrientation = ScreenOrientation.Landscape)]
	public class MainActivity : Activity
	{
		public const string C_MAIN_PREFS = @"C_MAIN_PREFS";
		public const string C_SAVED_PHARMACY_UUID = @"C_SAVED_PHARMACY_UUID";
		public const int C_ITEMS_IN_RESULT = 14;
		public const string C_DUMMY = @"C_DUMMY";

		string SelectedPharmacyUUID;

		List<Pharmacy> Pharmacies;
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
			CrashManager.Register(this, Secret.HockeyappAppId, new MyCrashManagerListener { ContextHolder = new WeakReference<Context>(this)});
			HockeyLog.LogLevel = 2;
			
			// Register to with the Update Manager
			UpdateManager.Register(this, Secret.HockeyappAppId);

			
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

			var isMillor = GetSharedPreferences(C_MAIN_PREFS, FileCreationMode.Private)
				.GetString(SigninDialog.C_HOST_URL, string.Empty)
				.Contains("milor");
			
			var add = FindViewById<ImageView>(Resource.Id.maAdd);
			add.Click += (sender, e) => {
				if (isMillor) {
					StartActivity(new Intent(this, typeof(PharmacyActivity)));
				} else {
					var isPharmacyAddEnable = MainDatabase.GetCustomizationBool(Customizations.IsPharmacyAddEnable);
					if (isPharmacyAddEnable.HasValue) {
						if (isPharmacyAddEnable.Value) {
							StartActivity(new Intent(this, typeof(PharmacyActivity)));
						}
					} else {
						Toast.MakeText(this, Resource.String.pharmacy_add_disabled, ToastLength.Long).Show();
					}
				}
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

			var coterie = FindViewById<ImageView>(Resource.Id.maCoterie);
			coterie.Click += (sender, e) => {
				StartActivity(new Intent(this, typeof(CoterieActivity)));
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
				Toast.MakeText(this, "maSearchSettingsIV Clicked", ToastLength.Short).Show();
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
						.PutBoolean("IS_ON", true)
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
					Toast.MakeText(this, "Message saved", ToastLength.Short).Show();
				};
			};

			message.LongClick += (sender, e) => {
				StartActivity(new Intent(this, typeof(MessageActivity)));
			};

			var delete = FindViewById<ImageView>(Resource.Id.maDelete);
			delete.Click += (sender, e) => {
				string msg = "Удаление аптек не доступно по причине проработки техпроцесса...";
				Toast.MakeText(this, msg, ToastLength.Long).Show();	
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
				SelectedPharmacyUUID = Pharmacies[e.Position - 1].UUID;
			}
			//throw new DivideByZeroException("My DivideByZeroException");
		}

		void RecreateAdapter(List<Pharmacy> inputList = null)
		{
			// ветка для фильтра
			if (inputList != null) {
				if (inputList.Count > 0) {
					Pharmacies = inputList;
					PharmacyTable.Adapter = new PharmacyAdapter(this, Pharmacies);
					//if (!string.IsNullOrEmpty(SelectedPharmacyUUID)) {
					//	int index = Pharmacies.FindIndex(ph => ph.UUID == SelectedPharmacyUUID);
					//	if (index > -1) {
					//		PharmacyTable.SetSelection(index);
					//	}
					//}
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
				//FilterContent.Text = Version;
				Filter.SetBackgroundColor(Android.Graphics.Color.Transparent);


				var orderMapState = new Dictionary<string, int> {
					{ PharmacyState.psActive.ToString("G"), 0 },
					{ PharmacyState.psReserve.ToString("G"), 1 },
					{ PharmacyState.psClose.ToString("G"), 2 }
				};

				// TODO: optimize work with route
				var now = DateTime.Now;
				var date = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, new TimeSpan(0, 0, 0));
				var routeItems = MainDatabase.GetRouteItems(date);

				//Проверка правильности маршрута
				routeItems = FixRoute(routeItems);

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
			if (!string.IsNullOrEmpty(SelectedPharmacyUUID)) {
				int index = Pharmacies.FindIndex(ph => ph.UUID == SelectedPharmacyUUID);
				if (index > -1) {
					PharmacyTable.SetSelection(index);
				}
			}
		}

		// TODO: сделать более лучший вариант
		List<RouteItem> FixRoute(List<RouteItem> currentRoute)
		{
			var dict = new Dictionary<string, int>();
			var result = new List<RouteItem>();

			foreach(var item in currentRoute) {
				if (!dict.ContainsKey(item.Pharmacy)) {
					dict.Add(item.Pharmacy, 1);
					result.Add(item);
				}
			}
			return result;
		}

		void Search(string text)
		{
			if (string.IsNullOrEmpty(text)) {
				SearchTable.Adapter = null;
				RecreateAdapter();
				return;
			}

			//SDiag.Debug.WriteLine(@"Search: startmemory{0}", System.Diagnostics.Process.GetCurrentProcess().WorkingSet64);

			var w = new SDiag.Stopwatch();
			w.Start();
			// 1 насыщениe
			//var searchItems = new List<SearchItem>()
			if (SearchItems == null) {
				SearchItems = new Dictionary<string, SearchItem>();
				var pharmacies = MainDatabase.GetItems<Pharmacy>();
				foreach (var pharmacy in pharmacies) {
					SearchItems.Add(
						pharmacy.UUID,
						new SearchItem(
							pharmacy.UUID,
							pharmacy.GetName(),
							string.IsNullOrEmpty(pharmacy.Subway) ? string.Empty : MainDatabase.GetItem<Subway>(pharmacy.Subway).name,
							string.IsNullOrEmpty(pharmacy.Region) ? string.Empty : MainDatabase.GetItem<Region>(pharmacy.Region).name,
							string.IsNullOrEmpty(pharmacy.Brand) ? string.Empty : pharmacy.Brand
						)
					);
				}
			}
			w.Stop();
			SDiag.Debug.WriteLine(@"Search: насыщение={0}", w.ElapsedMilliseconds);

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

				if (culture.CompareInfo.IndexOf(item.Value.Name, text, CompareOptions.IgnoreCase) >= 0) {
					item.Value.Match = string.Format(matchFormat, @"адрес=" + item.Value.Name);
					result.Add(item.Value);
					if (result.Count > C_ITEMS_IN_RESULT) break;
					continue;
				}
			}
			w.Stop();
			SDiag.Debug.WriteLine(@"Search: поиск={0}", w.ElapsedMilliseconds);

			w.Restart();
			// 3 показ
			SearchTable.Adapter = new SearchResultAdapter(this, result);
			//SearchTable.Adapter = new ArrayAdapter<string>(
			//	this, Android.Resource.Layout.SimpleListItemMultipleChoice, result.Select(si => si.Name).ToArray()
			//);
			w.Stop();
			SDiag.Debug.WriteLine(@"Search: показ={0}", w.ElapsedMilliseconds);
			//SDiag.Debug.WriteLine(@"Search: endmemory{0}", System.Diagnostics.Process.GetCurrentProcess().WorkingSet64);
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			outState.PutString("username", MainDatabase.Username);
			outState.PutString("selectedpharmacyuuid", SelectedPharmacyUUID);
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState) { 
			string username = savedInstanceState.GetString("username"); 
			MainDatabase.Username = username; 
			Helper.Username = username;
			SelectedPharmacyUUID = savedInstanceState.GetString("selectedpharmacyuuid");
			base.OnRestoreInstanceState(savedInstanceState); 
		}

		public Account GetSyncAccount(Context context)
		{
			//var newAccount = new Account(SyncConst.ACCOUNT, SyncConst.ACCOUNT_TYPE);

			var accountManager = (AccountManager)context.GetSystemService(AccountService);
			
			var accounts = accountManager.GetAccountsByType(SyncConst.ACCOUNT_TYPE);
			
			foreach(var acc in accounts){
				if (acc.name == SyncConst.ACCOUNT) return acc;
			}
			
			
//			var tag = "CRMLite:MainActivity:CreateSyncAccount";
//			if (accountManager.AddAccountExplicitly(newAccount, null, null)) {
//				Android.Util.Log.Info(tag, "AddAccountExplicitly");
//			} else {
//				Android.Util.Log.Info(tag, "NOT AddAccountExplicitly");
//			}

			return null;
		}
		
		protected override void OnResume()
		{
			base.OnResume();

			var shared = GetSharedPreferences(C_MAIN_PREFS, FileCreationMode.Private);

			Helper.CheckIfTimeChangedAndShowDialog(this);

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
						SDiag.Debug.WriteLine(@"DBPath = {0}", MainDatabase.DBPath);
					});
				};

				SDiag.Debug.WriteLine(@"username = IsNullOrEmpty");
				return;
			}

			var ft = FragmentManager.BeginTransaction();
			var sd = FragmentManager.FindFragmentByTag(SigninDialog.TAG);
			if (sd != null) {
				ft.Remove(sd);
			}
			ft.Commit();

			if (!IsLocationActive() || !IsInternetActive()) return;

			MainDatabase.Username = username;
			Helper.Username = username;

			var agentUUID = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);
			try {
				var agent = MainDatabase.GetItem<Agent>(agentUUID);
				MainDatabase.AgentUUID = agent.uuid;
				Helper.WeeksInRoute = agent.weeksInRout;
				Helper.WorkMode = agent.GetWorkMode();
				Helper.AndroidId = Helper.GetAndroidId(this);
			} catch (Exception ex) {
				SDiag.Debug.WriteLine(ex.Message);
			}

			var isLocatorEnable = MainDatabase.GetCustomizationBool(Customizations.IsLocatorEnable);
			if (isLocatorEnable.HasValue) {
				if (isLocatorEnable.Value) {
					var isLocatorGPSRequestOn = MainDatabase.GetCustomizationBool(Customizations.IsLocatorGPSRequestOn);
					var isLocatorNetRequestOn = MainDatabase.GetCustomizationBool(Customizations.IsLocatorNetRequestOn);
					var locatorGPSRequestPeriod = MainDatabase.GetCustomizationInt(Customizations.LocatorGPSRequestPeriod);
					var locatorNetRequestPeriod = MainDatabase.GetCustomizationInt(Customizations.LocatorNetRequestPeriod);
					var locatorIdlePeriod = MainDatabase.GetCustomizationInt(Customizations.LocatorIdlePeriod);

					var locator = new Intent(this, typeof(LocatorService));
					locator.PutExtra(MainDatabase.C_LOC_PATH, MainDatabase.LOCPath);
					locator.PutExtra(SigninDialog.C_AGENT_UUID, agentUUID);

					if (isLocatorGPSRequestOn.HasValue) {
						locator.PutExtra(Customizations.IsLocatorGPSRequestOn, isLocatorGPSRequestOn.Value);
					}

					if (isLocatorNetRequestOn.HasValue) {
						locator.PutExtra(Customizations.IsLocatorNetRequestOn, isLocatorNetRequestOn.Value);
					}

					if (locatorGPSRequestPeriod.HasValue) {
						locator.PutExtra(Customizations.LocatorGPSRequestPeriod, locatorGPSRequestPeriod.Value);
					}

					if (locatorNetRequestPeriod.HasValue) {
						locator.PutExtra(Customizations.LocatorNetRequestPeriod, locatorNetRequestPeriod.Value);
					}

					if (locatorIdlePeriod.HasValue) {
						locator.PutExtra(Customizations.LocatorIdlePeriod, locatorIdlePeriod.Value);
					}

					StartService(locator);
				} else {
					StopService(new Intent(this, typeof(LocatorService)));
				}
			}

			var isPhotoUploaderEnable = MainDatabase.GetCustomizationBool(Customizations.IsPhotoUploaderEnable);
			if (isPhotoUploaderEnable.HasValue) {
				if (isPhotoUploaderEnable.Value) {
					var photoUploader = new Intent(this, typeof(PhotoUploaderService));
					photoUploader.PutExtra(MainDatabase.C_DB_PATH, MainDatabase.DBPath);
					photoUploader.PutExtra(SigninDialog.C_AGENT_UUID, agentUUID);
					StartService(photoUploader);
				} else {
					StopService(new Intent(this, typeof(PhotoUploaderService)));
				}
			}


			// Find account and Request manual sync
			var account = GetSyncAccount(this);
			if (account != null) {
				var settingsBundle = new Bundle();
				settingsBundle.PutString(MainDatabase.C_DB_PATH, MainDatabase.DBPath);
				settingsBundle.PutString(MainDatabase.C_LOC_PATH, MainDatabase.LOCPath);
				settingsBundle.PutString(SigninDialog.C_ACCESS_TOKEN, ACCESS_TOKEN);
				settingsBundle.PutString(SigninDialog.C_HOST_URL, HOST_URL);
				settingsBundle.PutBoolean(ContentResolver.SyncExtrasExpedited, false);
				settingsBundle.PutBoolean(ContentResolver.SyncExtrasDoNotRetry, false);
				settingsBundle.PutBoolean(ContentResolver.SyncExtrasManual, true);

				ContentResolver.AddPeriodicSync(account, SyncConst.AUTHORITY, settingsBundle);				
			}

			
			var packageInfo = ApplicationContext.PackageManager.GetPackageInfo(ApplicationContext.PackageName, 0);
			var version = string.Format("Версия: {0} ({1})", packageInfo.VersionName, packageInfo.VersionCode);
			FindViewById<TextView>(Resource.Id.maVersionTV).Text = version;

			var w = new SDiag.Stopwatch();

		    w.Restart();
			var currentRouteItems = MainDatabase.GetRouteItems(DateTimeOffset.Now);

		    if (currentRouteItems.Count() > 0){
				var nextDate = DateTimeOffset.Now.AddDays(Helper.WeeksInRoute * 7);
				var date = new DateTimeOffset(nextDate.Year, nextDate.Month, nextDate.Day, 0, 0, 0, new TimeSpan(0, 0, 0));
				var nextRoutItems = MainDatabase.GetRouteItems(date);

		      if (nextRoutItems.Count() == 0) {

		        using (var trans = MainDatabase.BeginTransaction()){
		          foreach (var oldItem in currentRouteItems){
		            var newItem = MainDatabase.Create2<RouteItem>();
						      newItem.Pharmacy = oldItem.Pharmacy;
						      newItem.Date = date;
						      newItem.Order = oldItem.Order;
		          }
		          trans.Commit();
		        }
		      }
		    }
		    w.Stop();
		    SDiag.Debug.WriteLine(@"Route: копирование={0}", w.ElapsedMilliseconds);
    
			w.Restart();
			int count = MainDatabase.GetItems<Attendance>()
									.Count(att => att.When.LocalDateTime.Date == DateTimeOffset.Now.Date);
			AttendanceCount.Text = string.Format(
				@"РЕЖИМ РАБОТЫ: {0};  СЕГОДНЯ ВИЗИТОВ: {1}", Helper.GetWorkModeDesc(Helper.WorkMode), count
			);
			w.Stop();
			SDiag.Debug.WriteLine(@"OnResume: подсчет визитов={0}", w.ElapsedMilliseconds);

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

			if (SearchItems.ContainsKey(uuid)) {
				w.Start();
				var pharmacy = MainDatabase.GetEntity<Pharmacy>(uuid);
				SearchItems[uuid] = new SearchItem(
					pharmacy.UUID,
					pharmacy.GetName(),
					string.IsNullOrEmpty(pharmacy.Subway) ? string.Empty : MainDatabase.GetItem<Subway>(pharmacy.Subway).name,
					string.IsNullOrEmpty(pharmacy.Region) ? string.Empty : MainDatabase.GetItem<Region>(pharmacy.Region).name,
					string.IsNullOrEmpty(pharmacy.Brand) ? string.Empty : pharmacy.Brand
				);
			}
			prefs.Edit().PutString(C_SAVED_PHARMACY_UUID, string.Empty).Commit();
			w.Stop();
			SDiag.Debug.WriteLine(@"Search: обновление={0}", w.ElapsedMilliseconds);
		}

		bool IsLocationActive()
		{
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

		bool IsInternetActive()
		{
			var cm = GetSystemService(ConnectivityService) as ConnectivityManager;
			if (cm.ActiveNetworkInfo != null) {
				if (cm.ActiveNetworkInfo.IsConnectedOrConnecting) {
					return true;
				}
				new AlertDialog.Builder(this)
				               .SetTitle(Resource.String.error_caption)
				               .SetMessage(Resource.String.no_internet_connection)
				               .SetCancelable(false)
				               .SetNegativeButton(Resource.String.cancel_button, (sender, args) => {
								   if (sender is Dialog) {
									  (sender as Dialog).Dismiss();
								   }
								})
				               .Show();
				return false;
			}
			new AlertDialog.Builder(this)
			               .SetTitle(Resource.String.warning_caption)
			               .SetMessage(Resource.String.no_internet_provider)
			               .SetCancelable(false)
			               .SetPositiveButton(Resource.String.on_button, delegate {
								var intent = new Intent(Android.Provider.Settings.ActionWirelessSettings);
								StartActivity(intent);
							})
			               .Show();
			return false;
		}


		protected override void OnPause()
		{
			UpdateManager.Unregister();
			base.OnPause();
		}

		protected override void OnStop()
		{
			base.OnStop();
			UpdateManager.Unregister();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			UpdateManager.Unregister();
		}
	}
}


