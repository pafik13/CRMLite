using System;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Net;
using System.Net.Cache;
using System.Text.RegularExpressions;

using Android.App;
using Android.Widget;
using Android.Content;
using Android.Provider;

using RestSharp;

using CRMLite.Receivers;
 
namespace CRMLite
{
	public enum WorkMode
	{
		wmOnlyRoute, wmRouteAndRecommendations, wmOnlyRecommendations
	}

	public static class Extensions
	{
		// https://stackoverflow.com/questions/5320592/value-is-in-enum-list
		public static bool In<T>(this T val, params T[] values) where T : struct
		{
			return values.Contains(val);
		}
		
		// https://stackoverflow.com/questions/16100/how-should-i-convert-a-string-to-an-enum-in-c
		public static T ToEnum<T>(this string value, T defaultValue) where T : struct
		{
			if (string.IsNullOrEmpty(value))
			{
				return defaultValue;
			}

			T result;
			return Enum.TryParse(value, true, out result) ? result : defaultValue;
		}
	}
	
	public static class Helper
	{
		public static bool IsTimeChanged;

		public static string C_DB_FILE_NAME = "main.realm";
		public static string C_LOC_FILE_NAME = "location.realm";

		public static string Username = string.Empty;

		public static int WeeksInRoute = 3;

		public static WorkMode WorkMode = WorkMode.wmOnlyRoute;

		public static string AndroidId = string.Empty;

		public static string GetAndroidId(Context context)
		{
			return Settings.Secure.GetString(context.ContentResolver, Settings.Secure.AndroidId);
		}

		public static string AppDir {
			get {
				return Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, @"ru.sbl.crmlite");
			}
		}

		public static string PhotoDir {
			get {
				if (string.IsNullOrEmpty(Username)) return string.Empty;

				var photoDir = Path.Combine(AppDir, Username, @"photos");
				if (Directory.Exists(photoDir)) {
					Debug.WriteLine("That PhotoDir exists already.");
					return photoDir;
				}

				// Try to create the directory.
				DirectoryInfo di = Directory.CreateDirectory(photoDir);
				Debug.WriteLine("The PhotoDir was created successfully at {0}.", Directory.GetCreationTime(photoDir));

				return photoDir;
			}
		}

		public static string MaterialDir {
			get {
				if (string.IsNullOrEmpty(Username)) return string.Empty;

				var materialDir = Path.Combine(AppDir, Username, @"materials");
				if (Directory.Exists(materialDir)) {
					Debug.WriteLine("That MaterialDir exists already.");
					return materialDir;
				}

				// Try to create the directory.
				DirectoryInfo di = Directory.CreateDirectory(materialDir);
				Debug.WriteLine("The MaterialDir was created successfully at {0}.", Directory.GetCreationTime(materialDir));

				return materialDir;
			}
		}

		public static string LibraryDir {
			get {
				if (string.IsNullOrEmpty(Username)) return string.Empty;

				var libraryDir = Path.Combine(AppDir, Username, "library");
				if (Directory.Exists(libraryDir)) {
					Debug.WriteLine("That LibraryDir exists already.");
					return libraryDir;
				}

				// Try to create the directory.
				DirectoryInfo di = Directory.CreateDirectory(libraryDir);
				Debug.WriteLine("The LibraryDir was created successfully at {0}.", Directory.GetCreationTime(libraryDir));

				return libraryDir;
			}
		}

		public static string GetWorkModeDesc(WorkMode workMode)
		{
			switch (workMode) {
				case WorkMode.wmOnlyRoute:
					return "Строго по маршруту";
				case WorkMode.wmRouteAndRecommendations:
					return "Маршрут и рекомендации";
				case WorkMode.wmOnlyRecommendations:
					return "Только рекомендации";
				default:
					return @"<Unknown>";
			}
		}

		// This presumes that weeks start with Monday.
		// Week 1 is the 1st week of the year with a Thursday in it.
		public static int GetIso8601WeekOfYear(DateTime time)
		{
			// Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
			// be the same week# as whatever Thursday, Friday or Saturday are,
			// and we always get those right
			DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
			if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday) {
				time = time.AddDays(3);
			}

			// Return the week of our adjusted day
			return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
		}

		public static int? ToInt(string value, int divider = 1)
		{
			if (string.IsNullOrEmpty(value)) return null;

			int result;
			int.TryParse(value.Replace(',', '.'), NumberStyles.Integer, new CultureInfo("en-US").NumberFormat, out result);

			return result;
		}

		public static float? ToFloat(string value, int divider = 1)
		{
			if (string.IsNullOrEmpty(value)) return null;

			float result;
			float.TryParse(value.Replace(',', '.'), NumberStyles.Float, new CultureInfo("en-US").NumberFormat, out result);

			if (float.IsInfinity(result) || Math.Abs(result) < 0.01f) return null;

			result /= divider;

			if (float.IsInfinity(result) || Math.Abs(result) < 0.01f) return null;

			return result;
		}

		public static float ToFloatExeptNull(string value)
		{
			if (string.IsNullOrEmpty(value)) return 0.0f;

			float result;
			float.TryParse(value.Replace(',', '.'), NumberStyles.Float, new CultureInfo("en-US").NumberFormat, out result);

			if (float.IsInfinity(result) || Math.Abs(result) < 0.01f) return 0.0f; ;

			return result;
		}

		public static int ToIntExeptNull(string value)
		{
			if (string.IsNullOrEmpty(value)) return 0;

			int result;
			int.TryParse(value.Replace(',', '.'), NumberStyles.Integer, new CultureInfo("en-US").NumberFormat, out result);

			return result;
		}

		public static void CheckIfTimeChangedAndShowDialog(Context context)
		{
			var shared = context.GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private);

			IsTimeChanged = shared.GetBoolean(TimeChangedReceiver.C_IS_TIME_CHANGED, false);

			if (IsTimeChanged) {
				var dialog = new AlertDialog.Builder(context)
											.SetTitle(Resource.String.error_caption)
											.SetMessage(Resource.String.time_changed)
											.SetCancelable(false)
											.SetPositiveButton(Resource.String.check_button, (EventHandler<DialogClickEventArgs>)null)
											.Create();
				dialog.SetCanceledOnTouchOutside(false);
				dialog.SetCancelable(false);
				dialog.Show();
				//Overriding the handler immediately after show is probably a better approach than OnShowListener as described belo
				var dialogBtn = dialog.GetButton((int)DialogButtonType.Positive);
				dialogBtn.Click += (sender, args) => {
					// Don't dismiss dialog
					if (Math.Abs((GetServerTime() - DateTime.Now).TotalHours) < 1.1d) {
						IsTimeChanged = false;
						shared.Edit()
							  .PutBoolean(TimeChangedReceiver.C_IS_TIME_CHANGED, false)
							  .Commit();
						dialog.Dismiss();
					} else {
						Toast.MakeText(context, Resource.String.time_diff_more_than_hour, ToastLength.Long).Show();
					}
				};
			}
		}

		public static DateTime GetServerTime(RestClient restClient = null, RestRequest restRequest = null)
		{
			var sw = new Stopwatch();
			sw.Start();

			DateTime dateTime = DateTime.MinValue;

			var client = restClient ?? new RestClient("http://front-sblcrm.rhcloud.com/");
			client.Timeout = 10 * 1000;
			client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
			var request = restRequest ?? new RestRequest("time", Method.GET);
			var response = client.Execute(request);
			if (response.StatusCode == HttpStatusCode.OK) {
				Debug.WriteLine(response.Content);
				var w = new Stopwatch();
				w.Start();
				//StreamReader stream = new StreamReader(response.GetResponseStream());
				string time = Regex.Match(response.Content, @"(?<=\btime="")[^""]*").Value;
				double milliseconds = (time.Length > 14) ? Convert.ToInt64(time) / 1000.0 : Convert.ToInt64(time) / 1.0;
				dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds).ToLocalTime();
				w.Stop();
				Debug.WriteLine(time);
				Debug.WriteLine(dateTime);
				Debug.WriteLine(w.ElapsedMilliseconds);
			}

			sw.Stop();
			Debug.WriteLine(sw.ElapsedMilliseconds);

			return dateTime;
		}
	}
}
