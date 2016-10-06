﻿using System;
using System.Globalization;
using System.Diagnostics;
using System.IO;

using Android.Provider;
using Android.Content;

namespace CRMLite
{
	public enum WorkMode
	{
		wmOnlyRoute, wmRouteAndRecommendations, wmOnlyRecommendations
	}

	public static class Helper
	{
		public const string C_DB_FILE_NAME = @"main.realm";

		public static string Username = @"";

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
	}
}
