using System;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Content.PM;
using Android.Views.InputMethods;

using CRMLite.Dialogs;
using CRMLite.Adapters;
using CRMLite.Entities;

namespace CRMLite
{
	[Activity(Label = "ProfileActivity", ScreenOrientation = ScreenOrientation.Landscape)]
	public class ProfileActivity : Activity
	{
		LinearLayout Content;
		ListView Table;
		DateTimeOffset[] Dates;
		Dictionary<string, Dictionary<int, int>> ReportData;

		ViewSwitcher SearchSwitcher;

		ImageView SearchImage;

		EditText SearchEditor;

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

			FindViewById<Button>(Resource.Id.paExitAppB).Click += (sender, e) => {
				GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private)
					.Edit()
					.PutString(SigninDialog.C_USERNAME, string.Empty)
					.Commit();
				MainDatabase.Dispose();
				Finish();
			};

			Content = FindViewById<LinearLayout>(Resource.Id.paAttendanceByWeekLL);
			Table = FindViewById<ListView>(Resource.Id.paAttendanceByWeekTable);

			int weeksCount = 14;
			Dates = new DateTimeOffset[weeksCount];
			var header = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.AttendanceByWeekTableHeader, Table, false);
			(header.GetChildAt(0) as TextView).Text = @"Недели";
			for (int w = 0; w < weeksCount; w++) {
				Dates[w] = DateTimeOffset.UtcNow.AddDays(-7 * (weeksCount - 1 - w));
				var hView = header.GetChildAt(w + 1);
				if (hView is TextView) {
					(hView as TextView).Text = Helper.GetIso8601WeekOfYear(Dates[w].UtcDateTime).ToString();
				}
			}
			Content.AddView(header, 1);

			var shared = GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private);

			FindViewById<TextView>(Resource.Id.paUsernameTV).Text = shared.GetString(SigninDialog.C_USERNAME, string.Empty);

			var agentUUID = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);
			try {
				var agent = MainDatabase.GetItem<Agent>(agentUUID);
				FindViewById<TextView>(Resource.Id.paShortNameTV).Text = agent.shortName;
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}

			SearchSwitcher = FindViewById<ViewSwitcher>(Resource.Id.paSearchVS);
			SearchSwitcher.SetInAnimation(this, Android.Resource.Animation.SlideInLeft);
			SearchSwitcher.SetOutAnimation(this, Android.Resource.Animation.SlideOutRight);

			SearchImage = FindViewById<ImageView>(Resource.Id.paSearchIV);
			SearchImage.Click += (sender, e) => {
				if (CurrentFocus != null) {
					var imm = (InputMethodManager)GetSystemService(InputMethodService);
					imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.None);
				}

				SearchSwitcher.ShowNext();
			};

			SearchEditor = FindViewById<EditText>(Resource.Id.paSearchET);

			SearchEditor.AfterTextChanged += (sender, e) => {
				var text = e.Editable.ToString();

				(Table.Adapter as AttendanceByWeekAdapter).SetSearchText(text);
			};

		}

		protected override void OnResume()
		{
			base.OnResume();
			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			ReportData = MainDatabase.GetProfileReportData(Dates);

			int weeksCount = 14;
			var summer = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.AttendanceByWeekTableHeader, Table, false);
			(summer.GetChildAt(0) as TextView).Text = @"Итого";
			for (int w = 0; w < weeksCount; w++) {
				var d = Dates[w].UtcDateTime.Date;
				var key = d.Year * 100 + Helper.GetIso8601WeekOfYear(d);
				var hView = summer.GetChildAt(w + 1);
				if (hView is TextView) {
					int sum = 0;
					foreach (var item in ReportData) {
						sum += item.Value[key];
					}

					(hView as TextView).Text = sum.ToString();
				}
			}
			Content.AddView(summer, 2);

			Table.Adapter = new AttendanceByWeekAdapter(this, ReportData, Dates);
			
			watch.Stop();

			Console.WriteLine("OnResume: {0}", watch.ElapsedMilliseconds);
		}
	}
}

