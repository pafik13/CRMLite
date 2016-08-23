
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
	[Activity(Label = "HistoryActivity")]
	public class HistoryActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here

			SetContentView(Resource.Layout.History);

			FindViewById<Button>(Resource.Id.haCloseB).Click += (sender, e) => {
				Finish();
			};

			var date1 = FindViewById<TextView>(Resource.Id.htiDate1);
			date1.Text = DateTimeOffset.Now.Date.ToString("dd.MM.yy");

			var date2 = FindViewById<TextView>(Resource.Id.htiDate2);
			date2.Text = DateTimeOffset.Now.Date.AddDays(7).Date.ToString("dd.MM.yy");

			var table = FindViewById<LinearLayout>(Resource.Id.haTable);
			for (int i = 0; i < 60; i++) {
				var view = LayoutInflater.Inflate(Resource.Layout.HistoryTableItem, table, true);
			}
		}
	}
}

