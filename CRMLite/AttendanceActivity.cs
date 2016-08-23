using System;
using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;

using CRMLite.Dialogs;

namespace CRMLite
{
	[Activity(Label = "AttendanceActivity")]
	public class AttendanceActivity : FragmentActivity, ViewPager.IOnPageChangeListener
	{
		public const int NUM_PAGES = 4;

		/**
		 * The pager widget, which handles animation and allows swiping horizontally to access previous
		 * and next wizard steps.
		 */
		private ViewPager mPager;

		/**
		 * The pager adapter, which provides the pages to the view pager widget.
		 */
		private PagerAdapter mPagerAdapter;

		private TextView mTitle;

		public bool mIsAttendanceStart;

		string mPharmacyUUID;

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			//throw new NotImplementedException();
			return;
		}

		public void OnPageScrollStateChanged(int state)
		{
			//throw new NotImplementedException();
			return;
		}

		public void OnPageSelected(int position)
		{
			//throw new NotImplementedException();
			switch (position) {
				case 0:
					mTitle.Text = @"АПТЕКА";
					break;
				case 1:
					mTitle.Text = @"СОТРУДНИКИ";
					break;
				case 2:
					mTitle.Text = @"СОБИРАЕМАЯ ИНФОРМАЦИЯ";
					break;
				case 3:
					mTitle.Text = @"ФОТО НА ВИЗИТЕ";
					break;
				default:
					mTitle.Text = @"СТРАНИЦА " + (position + 1);
					break;;
			}
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			base.OnCreate(savedInstanceState);

			// Create your application here

			SetContentView(Resource.Layout.activity_screen_slide);

			mPharmacyUUID = Intent.GetStringExtra("UUID");


			mIsAttendanceStart = false;

			mTitle = FindViewById<TextView>(Resource.Id.Title);
			mTitle.Text = @"АПТЕКА";
			// Instantiate a ViewPager and a PagerAdapter.
			mPager = FindViewById<ViewPager>(Resource.Id.pager);
			mPager.AddOnPageChangeListener(this);
			mPagerAdapter = new ScreenSlidePagerAdapter(SupportFragmentManager);
			(mPagerAdapter as ScreenSlidePagerAdapter).mIsAttendanceStart = mIsAttendanceStart;
			(mPagerAdapter as ScreenSlidePagerAdapter).mPharmacyUUID = mPharmacyUUID;
			//(mPagerAdapter as ScreenSlidePagerAdapter).
			mPager.OffscreenPageLimit = 3;
			mPager.Adapter = mPagerAdapter;

			FindViewById<Button>(Resource.Id.StartAttendance).Click += delegate
			{
				mIsAttendanceStart = true;
				(mPagerAdapter as ScreenSlidePagerAdapter).mIsAttendanceStart = mIsAttendanceStart;
				mPager.CurrentItem = 0;
			};

			FindViewById<Button>(Resource.Id.CloseOrStopAttendance).Click += delegate
			{
				Finish();
			};
		}

		protected override void OnResume()
		{
			base.OnResume();

		//	var fragmentTransaction = FragmentManager.BeginTransaction();
		//	var prev = FragmentManager.FindFragmentByTag(LockDialog.TAG);
		//	if (prev != null)
		//	{
		//		fragmentTransaction.Remove(prev);
		//	}
		//	fragmentTransaction.AddToBackStack(null);

		//	var lockDialog = LockDialog.Create(@"МОЙ ПЕРВЫЙ ЛОКЕР");
		//	lockDialog.Cancelable = false;
		//	lockDialog.Show(fragmentTransaction, LockDialog.TAG);
		}

		protected override void OnStop()
		{
			base.OnStop();
		}

		/**
		 * A simple pager adapter that represents 5 {@link ScreenSlidePageFragment} objects, in
		 * sequence.
		 */
		private class ScreenSlidePagerAdapter : FragmentPagerAdapter
		{
			public bool mIsAttendanceStart;
			public string mPharmacyUUID;

			public ScreenSlidePagerAdapter (Android.Support.V4.App.FragmentManager fm) : base(fm)
			{
			}

			public override int Count
			{
				get
				{
					return NUM_PAGES;
				}
			}

			public override Android.Support.V4.App.Fragment GetItem(int position)
			{
				switch (position)
				{
					case 0:
						return PharmacyFragment.create(mPharmacyUUID);
					case 1:
						return EmployeeFragment.create(mPharmacyUUID);
					case 2:
						return InfoFragment.create(mPharmacyUUID);
					case 3:
						return PhotoFragment.create(mPharmacyUUID);
					default:
						return ScreenSlidePageFragment.create(position, mIsAttendanceStart); 
				}

			}
		}
	}
}

