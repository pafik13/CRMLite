
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CRMLite
{
	[Activity(Label = "RouteActivity", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape)]
	public class RouteActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
    
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Route);
		}
	}
}

