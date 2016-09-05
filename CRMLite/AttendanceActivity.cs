using System;
using System.Linq;
using Android.App;
using Android.OS;
using V4App = Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;

using CRMLite.Dialogs;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;

namespace CRMLite
{
	[Activity(Label = "AttendanceActivity")]
	public class AttendanceActivity : V4App.FragmentActivity, ViewPager.IOnPageChangeListener
	{
		public const int NUM_PAGES = 4;

		ViewPager Pager;
		TextView FragmentTitle;
		Button Close;
		ImageView Contracts;
		ImageView Finance;
		ImageView History;

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
					FragmentTitle.Text = @"АПТЕКА";
					break;
				case 1:
					FragmentTitle.Text = @"СОТРУДНИКИ";
					break;
				case 2:
					FragmentTitle.Text = @"СОБИРАЕМАЯ ИНФОРМАЦИЯ";
					break;
				case 3:
					FragmentTitle.Text = @"ФОТО НА ВИЗИТЕ";
					break;
				default:
					FragmentTitle.Text = @"СТРАНИЦА " + (position + 1);
					break;;
			}
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_screen_slide);

			PharmacyUUID = Intent.GetStringExtra("UUID");
			if (string.IsNullOrEmpty(PharmacyUUID)) return;

			AttendanceLast = MainDatabase.GetAttendaces(PharmacyUUID).OrderByDescending(i => i.When).FirstOrDefault();
			var attendanceLastUUID = AttendanceLast == null ? string.Empty : AttendanceLast.UUID;

			FragmentTitle = FindViewById<TextView>(Resource.Id.aaTitleTV);
			FragmentTitle.Text = @"АПТЕКА";

			Pager = FindViewById<ViewPager>(Resource.Id.aaContainerVP);
			Pager.AddOnPageChangeListener(this);
			Pager.OffscreenPageLimit = 3;
			Pager.Adapter = new AttendancePagerAdapter(SupportFragmentManager, PharmacyUUID, attendanceLastUUID);

			FindViewById<Button>(Resource.Id.aaStartOrStopAttendanceB).Click += (sender, e) =>
			{
				if (AttendanceStart == null) {
					AttendanceStart = DateTimeOffset.Now;

					for (int f = 0; f < NUM_PAGES; f++) {
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

					var button = sender as Button;
					button.SetBackgroundResource(Resource.Color.Deep_Orange_500);
					button.Text = "ЗАКОНЧИТЬ ВИЗИТ";
					return;
				}

				var fragmentTransaction = SupportFragmentManager.BeginTransaction();
				var prev = SupportFragmentManager.FindFragmentByTag(LockDialog.TAG);
				if (prev != null)
				{
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var lockDialog = LockDialog.Create("Идет сохранение данных...", Resource.Color.Deep_Orange_500);
				lockDialog.Cancelable = false;
				lockDialog.Show(fragmentTransaction, LockDialog.TAG);

				new Task(() => {
					Thread.Sleep(2000); // иначе не успеет показаться диалог

					RunOnUiThread(() => {
						//var transaction = MainDatabase.BeginTransaction();
						//var attendance = MainDatabase.Create<Attendance>();
						//attendance.Pharmacy = PharmacyUUID;
						//attendance.When = AttendanceStart.Value;

						////MainDatabase.SaveAttendace(Attendance);
						//for (int f = 0; f < NUM_PAGES; f++) {
						//	var fragment = GetFragment(f);
						//	if (fragment is IAttendanceControl) {
						//		(fragment as IAttendanceControl).OnAttendanceStop(transaction, attendance);
						//	}
						//}

						//transaction.Commit();
						lockDialog.Dismiss();
						Finish();
					});
				}).Start();
			};


			Close = FindViewById<Button>(Resource.Id.aaCloseB);
			Close.Click += (sender, e) =>
			{
				Finish();
			};

			Contracts = FindViewById<ImageView>(Resource.Id.aaContractsIV);
			Finance = FindViewById<ImageView>(Resource.Id.aaFinanceIV);
			Finance.Click += (sender, e) => {
				var financeAcivity = new Intent(this, typeof(FinanceActivity));
				financeAcivity.PutExtra(@"UUID", PharmacyUUID);
				StartActivity(financeAcivity);
			};

			History = FindViewById<ImageView>(Resource.Id.aaHistoryIV);
			History.Click += (sender, e) => {
				var historyAcivity = new Intent(this, typeof(HistoryActivity));
				historyAcivity.PutExtra(@"UUID", PharmacyUUID);
				StartActivity(historyAcivity);
			};
		}

		protected override void OnResume()
		{
			base.OnResume();
		}

		protected override void OnStop()
		{
			base.OnStop();
		}

		public override void OnBackPressed()
		{
			if (AttendanceStart == null) {
				base.OnBackPressed();
			}
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
					return NUM_PAGES;
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
						return PhotoFragment.create(PharmacyUUID);
					default:
						return ScreenSlidePageFragment.create(position, false);
				}

			}
		}
	}
}

