using Android.OS;
using Android.App;
using Android.Views;
using Android.Content;

namespace CRMLite
{
	//https://forums.xamarin.com/discussion/19362/xamarin-forms-splashscreen-in-android
	[Activity(Label = "CRMLite", Theme = "@style/MyTheme.Splash", Icon = "@mipmap/icon", MainLauncher = true, NoHistory = true)]
	public class SplashActivity : Activity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RequestWindowFeature (WindowFeatures.NoTitle);
			Window.AddFlags (WindowManagerFlags.KeepScreenOn);

			GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private).Edit()
			                                                                         .PutString(MainActivity.C_DUMMY, string.Empty)
			                                                                         .Commit();

			GetSharedPreferences(FilterDialog.C_FILTER_PREFS, FileCreationMode.Private).Edit()
			                                                                           .PutString(MainActivity.C_DUMMY, string.Empty)
			                                                                           .Commit();

			StartActivity(new Intent(this, typeof(MainActivity)));
		}
	}
}