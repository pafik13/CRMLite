using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;

namespace CRMLite
{
	[Activity(Label = "LibraryActivity")]
	public class LibraryActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Library);

			FindViewById<Button>(Resource.Id.laExitB).Click += (s, e) => {
				Finish();
			};
		}
	}
}

