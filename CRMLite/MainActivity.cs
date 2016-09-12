﻿using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Content;

using Realms;

using CRMLite.Entities;
using CRMLite.Adapters;
using System.Globalization;
using System.Diagnostics;
using System;
using Android.Views.InputMethods;

namespace CRMLite
{
	[Activity(Label = "Main")]
	public class MainActivity : Activity
	{
		public const string C_MAIN_PREFS = @"C_MAIN_PREFS";
		public const string C_SAVED_PHARMACY_UUID = @"C_SAVED_PHARMACY_UUID";
		public const int C_ITEMS_IN_RESULT = 10;

		IList<Pharmacy> Pharmacies;
		ListView PharmacyTable;
		TextView FilterContent;
		TextView AttendanceCount;
		EditText SearchInput;
		ListView SearchTable;
		Dictionary<string, SearchItem> SearchItems;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			if (Secret.IsNeedReCreateDB) {
				Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
			}

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			PharmacyTable = FindViewById<ListView>(Resource.Id.maPharmacyTable);
			View header = LayoutInflater.Inflate(Resource.Layout.PharmacyTableHeader, PharmacyTable, false);
			PharmacyTable.AddHeaderView(header);
			PharmacyTable.Clickable = true;
			PharmacyTable.ItemClick += OnListItemClick;

			var add = FindViewById<ImageView>(Resource.Id.maAdd);
			add.Click += delegate {
				StartActivity(new Intent(this, typeof(PharmacyActivity)));
			};

			var sync = FindViewById<ImageView>(Resource.Id.maSync);
			sync.Click += delegate {
				StartActivity(new Intent(this, typeof(SyncActivity)));
			};

			var lib = FindViewById<ImageView>(Resource.Id.maLibrary);
			lib.Click += delegate {
				StartActivity(new Intent(this, typeof(TestDataActivity)));
			};

			var route = FindViewById<ImageView>(Resource.Id.maRoute);
			route.Click += delegate {
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

			var filter = FindViewById<ImageView>(Resource.Id.maFilter);
			filter.Click += (sender, e) => {
				if (sender is ImageView) {
					var img = sender as ImageView;

					var isFilterOn = (bool)img.GetTag(Resource.String.IsFilterOn);
					isFilterOn = !isFilterOn;
					GetSharedPreferences(FilterDialog.C_FILTER_PREFS, FileCreationMode.Private)
						.Edit()
						.PutBoolean(@"IS_ON", isFilterOn)
						.Commit();

					if (isFilterOn) {
						img.SetBackgroundColor(Android.Graphics.Color.LightGreen);
					} else {
						img.SetBackgroundColor(Android.Graphics.Color.Transparent);
					}
					img.Invalidate();
					img.SetTag(Resource.String.IsFilterOn, isFilterOn);

					RecreateAdapter();
				}
			};

			filter.LongClick += (sender, e) => {
				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(FilterDialog.TAG);
				if (prev != null) {
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var filterDialog = new FilterDialog();
				filterDialog.Show(fragmentTransaction, FilterDialog.TAG);
				filterDialog.AfterSaved += (caller, arguments) => {
					RecreateAdapter();
				};
			};

			FilterContent = FindViewById<TextView>(Resource.Id.maFilterTV);
			AttendanceCount = FindViewById<TextView>(Resource.Id.maAttendanceCountTV);
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
			if (inputList != null) {
				if (inputList.Count > 0) {
					Pharmacies = inputList;
					PharmacyTable.Adapter = new PharmacyAdapter(this, Pharmacies);
					return;
				}
			}

			var list = MainDatabase.GetItems<Pharmacy>(); 

			var prefs = GetSharedPreferences(FilterDialog.C_FILTER_PREFS, FileCreationMode.Private);
			bool isOn = prefs.GetBoolean(@"IS_ON", false);
			string subway = prefs.GetString(@"SUBWAY", string.Empty);
			string region = prefs.GetString(@"REGION", string.Empty);
			string net = prefs.GetString(@"NET", string.Empty);
			string brand = prefs.GetString(@"BRAND", string.Empty);

			if (isOn) {
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
				if (!string.IsNullOrEmpty(brand)) {
					var culture = CultureInfo.GetCultureInfo("ru-RU");
					list = list.Where(ph => culture.CompareInfo.IndexOf(ph.Brand, brand, CompareOptions.IgnoreCase) >= 0
					                 ).ToList();
					filters.Add(string.Format(@"Бренд=""{0}""", brand));
				}

				FilterContent.Text = @"ФИЛЬТР: " + string.Join("; ", filters);
				FilterContent.Visibility = ViewStates.Visible;
				FilterContent.RequestFocus();
			} else {
				FilterContent.Visibility = ViewStates.Gone;
			}

			Pharmacies = list.Take(14).ToList();
			PharmacyTable.Adapter = new PharmacyAdapter(this, Pharmacies);
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
							MainDatabase.GetItem<Subway>(item.Subway).name,
							MainDatabase.GetItem<Region>(item.Region).name,
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
			var w = new Stopwatch();

			w.Restart();
			int count = MainDatabase.GetItems<Attendance>()
									.Count(att => att.When.LocalDateTime.Date == DateTimeOffset.Now.Date);
			AttendanceCount.Text = string.Format(@"СЕГОДНЯ ВИЗИТОВ: {0}", count);
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
				MainDatabase.GetItem<Subway>(pharmacy.Subway).name,
				MainDatabase.GetItem<Region>(pharmacy.Region).name,
				pharmacy.Brand
			);
			w.Stop();
			Console.WriteLine(@"Search: обновление={0}", w.ElapsedMilliseconds);
		}

		protected override void OnPause()
		{
			base.OnPause();
		}

		protected override void OnStop()
		{
			base.OnStop();
		}
	}
}


