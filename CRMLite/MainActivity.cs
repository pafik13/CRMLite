using System.Collections.Generic;
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

namespace CRMLite
{
	[Activity(Label = "Main")]
	public class MainActivity : Activity
	{

		IList<Pharmacy> Pharmacies;
		ListView PharmacyTable;
		TextView FilterContent;
		EditText SearchInput;
		ListView SearchTable;
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

		void RecreateAdapter()
		{
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
				return;
			}

			// 1 насыщениeе
			var searchItems = new List<SearchItem>();
			var pharmacies = MainDatabase.GetItems<Pharmacy>();
			foreach (var item in pharmacies) {
				searchItems.Add(
					new SearchItem(
						item.UUID,
						item.GetName(),
						MainDatabase.GetItem<Subway>(item.Subway).name,
						MainDatabase.GetItem<Region>(item.Region).name
					)
				);
			}

			var matchFormat = @"Совпадение: {0}";
			var result = new List<SearchItem>();
			var culture = CultureInfo.GetCultureInfo("ru-RU");
			// 2 поиск
			foreach (var item in searchItems) {
				if (culture.CompareInfo.IndexOf(item.Subway, text, CompareOptions.IgnoreCase) >= 0) {
					item.Match = string.Format(matchFormat, @"метро=" + item.Subway);
					result.Add(item);
					if (result.Count > 4) break;
					continue;
				}

				if (culture.CompareInfo.IndexOf(item.Region, text, CompareOptions.IgnoreCase) >= 0) {
					item.Match = string.Format(matchFormat, @"район=" + item.Region);
					result.Add(item);
					if (result.Count > 4) break;
					continue;
				}
			}
			// 3 показ
			SearchTable.Adapter = new SearchResultAdapter(this, result);
		}

		protected override void OnResume()
		{
			base.OnResume();

			RecreateAdapter();
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


