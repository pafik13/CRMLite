using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SD = System.Diagnostics;

using Android.OS;
using Android.Net;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.Views.InputMethods;
using Android.Support.V4.View;
using V4App = Android.Support.V4.App;

using CRMLite.Dialogs;
using CRMLite.Entities;
using CRMLite.Services;

namespace CRMLite
{
	[Activity(Label = "AttendanceActivity", ScreenOrientation=ScreenOrientation.Landscape, WindowSoftInputMode=SoftInput.AdjustPan)]
	public class AttendanceActivity : V4App.FragmentActivity, ViewPager.IOnPageChangeListener, ILocationListener
	{
		public const int C_NUM_PAGES = 4;

		string AgentUUID { get; set; }

		ViewPager Pager;
		TextView FragmentTitle;
		TextView TimerText;
		Timer Timer;
		int? TimerMin;
		int? TimerMS;
		Button Close;
		ImageView MakePhotoAfter;
		ImageView Contracts;
		ImageView Finance;
		ImageView History;
		ImageView Material;
		IList<Material> Materials;

		ImageView Distributors;
		IList<Distributor> DistributorsList;

		LocationManager LocMgr;
		List<Location> Locations;

		string PharmacyUUID;

		DateTimeOffset? AttendanceStart;
		Attendance AttendanceLast;

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			return;
		}

		public void OnPageScrollStateChanged(int state)
		{
			return;
		}

		public void OnPageSelected(int position)
		{
			switch (position) {
				case 0:
					FragmentTitle.Text = "АПТЕКА";
					break;
				case 1:
					FragmentTitle.Text = "СОТРУДНИКИ";
					break;
				case 2:
					FragmentTitle.Text = (AttendanceLast == null) || AttendanceStart.HasValue ? 
						"СОБИРАЕМАЯ ИНФОРМАЦИЯ" : string.Concat("ИНФОРМАЦИЯ С ВИЗИТА ОТ ", AttendanceLast.When.LocalDateTime);
					var w = new SD.Stopwatch();
					w.Start();
					var emp = GetFragment(1);
					if (emp is EmployeeFragment) {
						((EmployeeFragment)emp).SaveAllEmployees();
					}
					var info = GetFragment(2);
					if (info is InfoFragment) {
						((InfoFragment)info).RefreshEmployees();
					}
					w.Stop();
					SD.Debug.WriteLine("OnPageSelected: {0}", w.ElapsedMilliseconds);
					break;
				case 3:
					FragmentTitle.Text = (AttendanceLast == null) || AttendanceStart.HasValue ?
						"ФОТО НА ВИЗИТЕ" : string.Concat("ФОТО С ВИЗИТА ОТ ", AttendanceLast.When.LocalDateTime);
					break;
				default:
					FragmentTitle.Text = string.Concat("СТРАНИЦА ", (position + 1));
					break;;
			}
		}

		void HandleTimerCallback(object state)
		{
			var start = (DateTime)state;
			var remain = (DateTime.Now - start).TotalMilliseconds - TimerMS;
			var interval = TimeSpan.FromMilliseconds(remain.Value);
			var text = string.Empty;
			RunOnUiThread(() => {
				if (remain < 0) 
				{
					text = string.Concat("Осталось ", Math.Abs(interval.Minutes), " мин. ", Math.Abs(interval.Seconds), " сек.");
				} else 
				{
					TimerMin = null;
					TimerText.SetTextColor(Android.Graphics.Color.Black);
					text = string.Concat("Сверх нормы ", interval.Minutes, " мин. ", interval.Seconds, " сек.");
				}
				TimerText.Text = text;
			});
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_screen_slide);

			PharmacyUUID = Intent.GetStringExtra("UUID");
			if (string.IsNullOrEmpty(PharmacyUUID)) return;

			var mainSharedPreferences = GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private);
			AgentUUID = mainSharedPreferences.GetString(SigninDialog.C_AGENT_UUID, string.Empty);

			AttendanceLast = MainDatabase.GetAttendaces(PharmacyUUID).OrderByDescending(i => i.When).FirstOrDefault();
			var attendanceLastUUID = AttendanceLast == null ? string.Empty : AttendanceLast.UUID;

			Materials = MainDatabase.GetItems<Material>();

			DistributorsList = MainDatabase.GetItems<Distributor>();

			FragmentTitle = FindViewById<TextView>(Resource.Id.aaTitleTV);
			FragmentTitle.Text = "АПТЕКА";

			TimerText = FindViewById<TextView>(Resource.Id.aaTimerTV);
			TimerMin = MainDatabase.GetCustomizationInt(Customizations.AttendanceMinPeriod);

			if (TimerMin.HasValue) {
				TimerMS = TimerMin.Value * 60 * 1000;
				TimerText.Text = TimerText.Text = string.Concat("Осталось ", TimerMin.Value.ToString(), " мин. ", 00, " сек.");
			} else {
				TimerText.Visibility = ViewStates.Invisible;
			}
									
			Pager = FindViewById<ViewPager>(Resource.Id.aaContainerVP);
			Pager.AddOnPageChangeListener(this);
			Pager.OffscreenPageLimit = 3;
			Pager.Adapter = new AttendancePagerAdapter(SupportFragmentManager, PharmacyUUID, attendanceLastUUID);

			var btnStartStop = FindViewById<Button>(Resource.Id.aaStartOrStopAttendanceB);
			btnStartStop.Click += (sender, e) =>
			{
				var btn = sender as Button;
				btn.Enabled = false;

				if (!IsLocationActive() || !IsInternetActive()) {
					btn.Enabled = true;
					return;
				}

				if (AttendanceStart == null) {
					StopService(new Intent(this, typeof(LocatorService)));
					StopService(new Intent(this, typeof(PhotoUploaderService)));

					AttendanceStart = DateTimeOffset.Now;

					MakePhotoAfter.Visibility = ViewStates.Gone;

					// Location
					Locations = new List<Location>();

					LocMgr = GetSystemService(LocationService) as LocationManager;
					var locationCriteria = new Criteria();
					locationCriteria.Accuracy = Accuracy.Coarse;
					locationCriteria.PowerRequirement = Power.Medium;
					string locationProvider = LocMgr.GetBestProvider(locationCriteria, true);
					SD.Debug.WriteLine("Starting location updates with " + locationProvider);
					LocMgr.RequestLocationUpdates(locationProvider, 5000, 1, this);
					// !Location	

					if (Pager.CurrentItem == 2) {
						FragmentTitle.Text = @"СОБИРАЕМАЯ ИНФОРМАЦИЯ";
					}

					if (Pager.CurrentItem == 3) {
						FragmentTitle.Text = @"ФОТО НА ВИЗИТЕ";
					}

					for (int f = 0; f < C_NUM_PAGES; f++) {
						var fragment = GetFragment(f);
						if (fragment is IAttendanceControl) {
							(fragment as IAttendanceControl).OnAttendanceStart(AttendanceStart);
						}
					}

					Close.Visibility = ViewStates.Invisible;

					Contracts.Visibility = ViewStates.Visible;

					Finance.Visibility = ViewStates.Visible;
					var financeParams = Finance.LayoutParameters as RelativeLayout.LayoutParams;
					financeParams.AddRule(LayoutRules.LeftOf, Resource.Id.aaContractsIV);

					History.Visibility = ViewStates.Visible;
					var historyParams = History.LayoutParameters as RelativeLayout.LayoutParams;
					historyParams.AddRule(LayoutRules.LeftOf, Resource.Id.aaFinanceIV);

					Material.Visibility = ViewStates.Visible;
					var materialParams = Material.LayoutParameters as RelativeLayout.LayoutParams;
					materialParams.AddRule(LayoutRules.LeftOf, Resource.Id.aaHistoryIV);

					Distributors.Visibility = ViewStates.Visible;
					var distributorsParams = Distributors.LayoutParameters as RelativeLayout.LayoutParams;
					distributorsParams.AddRule(LayoutRules.LeftOf, Resource.Id.aaMaterialIV);

					var button = sender as Button;
					button.SetBackgroundResource(Resource.Color.Deep_Orange_500);
					button.Text = "ЗАКОНЧИТЬ ВИЗИТ";

					if (TimerMin.HasValue) {
						Timer = new Timer(HandleTimerCallback, DateTime.Now, 100, 1000);
					}

					btn.Enabled = true;
					return;
				}

				if (TimerMin.HasValue) {
					Toast.MakeText(this, "Не прошло минимально необходимое время визита...", ToastLength.Short).Show();
					btn.Enabled = true;
					return;
				}

				if ((DateTimeOffset.Now - AttendanceStart.Value).TotalSeconds < 30) {
					btn.Enabled = true;
					return;
				}


				if (CurrentFocus != null) {
					var imm = (InputMethodManager)GetSystemService(InputMethodService);
					imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.None);
				}

				var fragmentTransaction = SupportFragmentManager.BeginTransaction();
				var prev = SupportFragmentManager.FindFragmentByTag(LockDialog.TAG);
				if (prev != null)
				{
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);


				// Оповещаем фрагменты о завершении визита
				string undonePhotoTypes = string.Empty;
				for (int f = 0; f < C_NUM_PAGES; f++) {
					var fragment = GetFragment(f);
					if (fragment is PhotoFragment) {
						undonePhotoTypes = (fragment as PhotoFragment).GetUndonePhotoTypes();
					}
				}

				if (!string.IsNullOrEmpty(undonePhotoTypes)) {
					new AlertDialog.Builder(this)
								   .SetTitle(Resource.String.error_caption)
					               .SetMessage("Необходимо сделать следующие фото:" + System.Environment.NewLine + undonePhotoTypes)
								   .SetCancelable(false)
								   .SetPositiveButton(@"OK", (dialog, args) => {
									   if (dialog is Dialog) {
										   ((Dialog)dialog).Dismiss();
									   }
								   })
								   .Show();
					btn.Enabled = true;
					return;
				}

				var lockDialog = LockDialog.Create("Идет сохранение данных...", Resource.Color.Deep_Orange_500);
				lockDialog.Cancelable = false;
				lockDialog.Show(fragmentTransaction, LockDialog.TAG);
				if (TimerMin.HasValue) {
					Timer.Dispose();
				}
				LocMgr.RemoveUpdates(this);

				new Task(() => {
					Thread.Sleep(2000); // иначе не успеет показаться диалог

					RunOnUiThread(() => {
						var transaction = MainDatabase.BeginTransaction();
						var attendance = MainDatabase.Create2<Attendance>();
						attendance.Pharmacy = PharmacyUUID;
						attendance.When = AttendanceStart.Value;
						attendance.Duration = (DateTimeOffset.Now - AttendanceStart.Value).TotalMilliseconds;

						foreach (var location in Locations) {
							var gps = MainDatabase.Create2<GPSData>();
							gps.Attendance = attendance.UUID;
							gps.Accuracy = location.Accuracy;
							gps.Altitude = location.Altitude;
							gps.Bearing = location.Bearing;
							gps.ElapsedRealtimeNanos = location.ElapsedRealtimeNanos;
							gps.IsFromMockProvider = location.IsFromMockProvider;
							gps.Latitude = location.Latitude;
							gps.Longitude = location.Longitude;
							gps.Provider = location.Provider;
							gps.Speed = location.Speed;
						}

						var distributorUUIDs = (string)Distributors.GetTag(Resource.String.DistributorUUIDs);

						if (!string.IsNullOrEmpty(distributorUUIDs)) {
							foreach (var distributor in distributorUUIDs.Split(';')) {
								var distributorData = MainDatabase.Create2<DistributorData>();
								distributorData.Attendance = attendance.UUID;
								distributorData.Distributor = distributor;
							}	
						}

						// Оповещаем фрагменты о завершении визита
						for (int f = 0; f < C_NUM_PAGES; f++) {
							var fragment = GetFragment(f);
							if (fragment is IAttendanceControl) {
								(fragment as IAttendanceControl).OnAttendanceStop(transaction, attendance);
							}
						}

						transaction.Commit();

						// Try to collect unused objects
						GC.Collect();

						Finish();
					});
				}).Start();
			};

			MakePhotoAfter = FindViewById<ImageView>(Resource.Id.aaMakePhotoAfterAttendanceIV);

			// TODO: uncomment
			if (AttendanceLast != null) {
				if (AttendanceLast.When.Date == DateTimeOffset.UtcNow.Date) {
					btnStartStop.Visibility = ViewStates.Gone;
					TimerText.Visibility = ViewStates.Gone;
				}

				var afterAttPhotos = MainDatabase.GetItems<PhotoAfterAttendance>().Select(paa => paa.photoType).ToArray();
				if (afterAttPhotos.Count() > 0) {
					MakePhotoAfter.Visibility = ViewStates.Visible;
					MakePhotoAfter.Click += (sender, e) => {
						var types = MainDatabase.GetItems<PhotoType>()
						                        .Where(pt => afterAttPhotos.Contains(pt.uuid) && !pt.isNeedBrand)
						                        .ToList();
						new AlertDialog.Builder(this)
									   .SetTitle("Выберите тип фотографии:")
									   .SetCancelable(true)
									   .SetItems(
						                   types.Select(item => item.name).ToArray(),
										   (caller, arguments) => {
											   string typeUUID = types[arguments.Which].uuid;
											   for (int f = 0; f < C_NUM_PAGES; f++) {
												   var fragment = GetFragment(f);
												   if (fragment is IMakePhotoAfterAttendance) {
														(fragment as IMakePhotoAfterAttendance).MakePhotoAfterAttendance(typeUUID);
												   }
											   }
										   })
									   .Show();

					};
				}
			}

			Close = FindViewById<Button>(Resource.Id.aaCloseB);
			Close.Click += (sender, e) =>
			{
				Finish();
			};

			Contracts = FindViewById<ImageView>(Resource.Id.aaContractsIV);
			Contracts.Click += (sender, e) => {
				var contractActivity = new Intent(this, typeof(ContractActivity));
				contractActivity.PutExtra(ContractActivity.C_PHARMACY_UUID, PharmacyUUID);
				StartActivity(contractActivity);
			};

			Finance = FindViewById<ImageView>(Resource.Id.aaFinanceIV);
			Finance.Click += (sender, e) => {
				var financeActivity = new Intent(this, typeof(FinanceActivity));
				financeActivity.PutExtra(@"UUID", PharmacyUUID);
				financeActivity.PutExtra(FinanceActivity.C_IS_CAN_ADD, false);
				StartActivity(financeActivity);
			};

			History = FindViewById<ImageView>(Resource.Id.aaHistoryIV);
			History.Click += (sender, e) => {
				var historyActivity = new Intent(this, typeof(HistoryActivity));
				historyActivity.PutExtra(@"UUID", PharmacyUUID);
				StartActivity(historyActivity);
			};

			Material = FindViewById<ImageView>(Resource.Id.aaMaterialIV);
			Material.Click += (sender, e) => {
				if (Materials.Count == 0) return;

				var materialItems = new List<MaterialItem>();
				var materials = MainDatabase.GetMaterials(MainDatabase.GetItem<Agent>(AgentUUID).MaterialType);

				foreach (var material in materials) {
					var file = material.GetJavaFile();
					if (file.Exists() && file.Length() > 0) {
						materialItems.Add(new MaterialItem(material.name, file));
					}
				}

				if (materialItems.Count == 0) return;

				materialItems.Sort((x, y) => x.MaterialName.CompareTo(y.MaterialName));
				
				new AlertDialog.Builder(this)
				               .SetTitle("Выберите материал для показа:")
				               .SetCancelable(true)
				               .SetItems(
					               materialItems.Select(item => item.MaterialName).ToArray(), 
					               (caller, arguments) => {
										var intent = new Intent(Intent.ActionView);
									    var uri = Android.Net.Uri.FromFile(materialItems[arguments.Which].FilePath);
										intent.SetDataAndType(uri, "application/pdf");
								   		intent.SetFlags(ActivityFlags.NoHistory);
								   		StartActivity(intent);
							   })
				               .Show();
			};

			Distributors = FindViewById<ImageView>(Resource.Id.aaDistributorsIV);
			Distributors.Click += (sender, e) => {
				if (DistributorsList.Count == 0) return;

				var iv = (ImageView)sender;
				var distributorUUIDs = (string)iv.GetTag(Resource.String.DistributorUUIDs);
				var cacheDistributorUUIDs = string.IsNullOrEmpty(distributorUUIDs) ? new List<string>() : distributorUUIDs.Split(';').ToList();

				bool[] checkedItems = new bool[DistributorsList.Count];
				if (cacheDistributorUUIDs.Count > 0) {
					for (int i = 0; i < DistributorsList.Count; i++) {
						checkedItems[i] = cacheDistributorUUIDs.Contains(DistributorsList[i].uuid);
					}
				}

				new AlertDialog.Builder(this)
							   .SetTitle("Выберите дистрибьюторов:")
							   .SetCancelable(false)
							   .SetMultiChoiceItems(
								   DistributorsList.Select(item => item.name).ToArray(),
								   checkedItems,
								   (caller, arguments) => {
									   if (arguments.IsChecked) {
										   cacheDistributorUUIDs.Add(DistributorsList[arguments.Which].uuid);
									   } else {
										   cacheDistributorUUIDs.Remove(DistributorsList[arguments.Which].uuid);
									   }
								   }
							   )
							   .SetPositiveButton(
								   @"Сохранить",
								   (caller, arguments) => {
									   iv.SetTag(Resource.String.DistributorUUIDs, string.Join(";", cacheDistributorUUIDs));
										(caller as Dialog).Dispose();
								   }
								)
								.SetNegativeButton(@"Отмена", (caller, arguments) => { (caller as Dialog).Dispose(); })
								.Show();
			};
		}

		void MakeFullScreen()
		{
			View decorView = Window.DecorView;
			var uiOptions = (int)decorView.SystemUiVisibility;
			var newUiOptions = uiOptions;

			newUiOptions |= (int)SystemUiFlags.LayoutStable;
			newUiOptions |= (int)SystemUiFlags.LayoutHideNavigation;
			newUiOptions |= (int)SystemUiFlags.LayoutFullscreen;
			newUiOptions |= (int)SystemUiFlags.HideNavigation;
			newUiOptions |= (int)SystemUiFlags.Fullscreen;
			newUiOptions |= (int)SystemUiFlags.Immersive;

			decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
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

		protected override void OnResume()
		{
			base.OnResume();
			//if (AttendanceStart.HasValue) {
			//	MakeFullScreen();
			//}
			Helper.CheckIfTimeChangedAndShowDialog(this);
		}

		protected override void OnPause()
		{
			base.OnPause();

			// MainDatabase.Dispose();
		}

		protected override void OnStop()
		{
			base.OnStop();
		}

		public override void OnBackPressed()
		{
			if (AttendanceStart.HasValue) return;

			base.OnBackPressed();
		}

		/**
		 * @param containerViewId the ViewPager this adapter is being supplied to
		 * @param id pass in getItemId(position) as this is whats used internally in this class
		 * @return the tag used for this pages fragment
		 */
		public string MakeFragmentName(int containerViewId, long id)
		{
			return "android:switcher:" + containerViewId + ":" + id;
		}

		/**
		 * @return may return null if the fragment has not been instantiated yet for that position - this depends on if the fragment has been viewed
		 * yet OR is a sibling covered by {@link android.support.v4.view.ViewPager#setOffscreenPageLimit(int)}. Can use this to call methods on
		 * the current positions fragment.
		 */
		public V4App.Fragment GetFragment(int position)
		{
			string tag = MakeFragmentName(Pager.Id, position);
			var fragment = SupportFragmentManager.FindFragmentByTag(tag);
			return fragment;
		}

		public void OnLocationChanged(Location location)
		{
			Locations.Add(location);
		}

		public void OnProviderDisabled(string provider)
		{
			SD.Debug.WriteLine(provider + " disabled by user");
		}
		public void OnProviderEnabled(string provider)
		{
			SD.Debug.WriteLine(provider + " enabled by user");
		}
		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
			SD.Debug.WriteLine(provider + " availability has changed to " + status);
		}

		/**
		 * A pager adapter that represents <NUM_PAGES> fragments, in sequence.
		 */
		class AttendancePagerAdapter : V4App.FragmentPagerAdapter
		{
			readonly string PharmacyUUID;
			readonly string AttendanceLastUUID;

			public AttendancePagerAdapter(V4App.FragmentManager fm, string pharmacyUUID, string attendanceLastUUID) : base(fm)
			{
				PharmacyUUID = pharmacyUUID;
				AttendanceLastUUID = attendanceLastUUID;
			}

			public override int Count {
				get {
					return C_NUM_PAGES;
				}
			}

			public override V4App.Fragment GetItem(int position)
			{
				switch (position) {
					case 0:
						return PharmacyFragment.create(PharmacyUUID);
					case 1:
						return EmployeeFragment.create(PharmacyUUID);
					case 2:
						return InfoFragment.create(PharmacyUUID, AttendanceLastUUID);
					case 3:
						return PhotoFragment.create(PharmacyUUID, AttendanceLastUUID);
					default:
						return ScreenSlidePageFragment.create(position, false);
				}

			}
		}
	}
}

