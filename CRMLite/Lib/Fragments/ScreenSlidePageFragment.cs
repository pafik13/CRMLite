using Android.OS;
using Android.Views;
using Android.Widget;

using Android.Support.V4.App;

using CRMLite.Dialogs;

namespace CRMLite
{
	public class ScreenSlidePageFragment : Fragment
	{
		/**
		 * The argument key for the page number this fragment represents.
		 */
		public const string ARG_PAGE = "page";

		public const string ARG_IS_ATTENDANCE_START = "ARG_IS_ATTENDANCE_START";

	    /**
	     * The fragment's page number, which is set to the argument value for {@link #ARG_PAGE}.
	     */
	    int mPageNumber;

		bool mIsAttendanceStart;

		/**
		 * Factory method for this fragment class. Constructs a new fragment for the given page number.
		 */
		public static ScreenSlidePageFragment create(int pageNumber, bool isAttendanceStart)
		{
			ScreenSlidePageFragment fragment = new ScreenSlidePageFragment();
			Bundle args = new Bundle();
			args.PutInt(ARG_PAGE, pageNumber);
			args.PutBoolean(ARG_IS_ATTENDANCE_START, isAttendanceStart);
			fragment.Arguments = args;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
			mPageNumber = Arguments.GetInt(ARG_PAGE);
			mIsAttendanceStart = Arguments.GetBoolean(ARG_IS_ATTENDANCE_START, false);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);

			// Inflate the layout containing a title and body text.
			ViewGroup rootView = (ViewGroup)inflater
					.Inflate(Resource.Layout.fragment_screen_slide_page, container, false);

			// Set the title view to show the page number.
			var text = rootView.FindViewById<TextView>(Android.Resource.Id.Text1);
			text.Text = @"СТРАНИЦА " + (mPageNumber + 1);

			//if (mPageNumber % 2 == 0)
			if (!mIsAttendanceStart && mPageNumber > 1)
			{
				var lock_message = rootView.FindViewById<TextView>(Resource.Id.lock_message);
				lock_message.Text = @"БЛОКИРОВКА " + (mPageNumber + 1);
			}
			else
			{
				var locker = rootView.FindViewById<RelativeLayout>(Resource.Id.locker);
				locker.Visibility = ViewStates.Invisible;
			}

			return rootView;
		}

		public override void OnResume()
		{
			base.OnResume();

			//var fragmentTransaction = FragmentManager.BeginTransaction();
			//var prev = FragmentManager.FindFragmentByTag(LockDialog.TAG);
			//if (prev != null)
			//{
			//	fragmentTransaction.Remove(prev);
			//}
			//fragmentTransaction.AddToBackStack(null);

			//var lockDialog = LockDialog.Create(@"МОЙ ПЕРВЫЙ ЛОКЕР");
			//lockDialog.Cancelable = false;
			//lockDialog.Show(fragmentTransaction, LockDialog.TAG);
		}
	}
}

