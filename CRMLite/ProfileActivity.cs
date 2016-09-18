using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;
using CRMLite.Adapters;

namespace CRMLite
{
	[Activity(Label = "ProfileActivity")]
	public class ProfileActivity : Activity
	{
		ListView Table;
		DateTimeOffset[] Dates;
		Dictionary<string, Dictionary<int, int>> ReportData;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Profile);

			FindViewById<Button>(Resource.Id.paCloseB).Click += (sender, e) => {
				Finish();
			};

			Table = FindViewById<ListView>(Resource.Id.paAttendanceByWeekTable);

			int weeksCount = 14;
			Dates = new DateTimeOffset[weeksCount];
			var header = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.AttendanceByWeekTableHeader, Table, false);
			for (int w = 0; w < weeksCount; w++) {
				Dates[w] = DateTimeOffset.UtcNow.AddDays(-7 * (weeksCount - 1 - w));
				var hView = header.GetChildAt(w + 1);
				if (hView is TextView) {
					(hView as TextView).Text = Helper.GetIso8601WeekOfYear(Dates[w].UtcDateTime).ToString();
				}
			}
			FindViewById<LinearLayout>(Resource.Id.paAttendanceByWeekLL).AddView(header, 1);

			//Table.AddHeaderView(header);

			//var agentUUID = GetSharedPreferences(MainActivity.C_MAIN_PREFS)
			//				.GetString(C_AGENT_UUID, string.Empty);

			//if (string.IsEmptyOrNull(agentUUID)) return;

			//var agent = MainDatabase.GetEntity<Agent>();
			//FindViewById<TextView>(Resource.Id.paAgentShortNameTV).Text = agent.ShortName;
			//FindViewById<TextView>(Resource.Id.paAgentBirthDateTV).Text = agent.BirthDate.ToLongString();
			//FindViewById<TextView>(Resource.Id.paAgentPositionTV).Text = agent.Position;

			//FindViewById<Button>(Resource.Id.paExitApplicationB).Click += (sender, e) => {
			//	Exit_App();
			//};
		}

		protected override void OnResume()
		{
			base.OnResume();
			var watch = new Stopwatch();
			watch.Start();
			ReportData = MainDatabase.GetProfileReportData(Dates);
			Table.Adapter = new AttendanceByWeekAdapter(this, ReportData, Dates);
			
			watch.Stop();

			Console.WriteLine("OnResume: {0}", watch.ElapsedMilliseconds);
		}
	}
}

