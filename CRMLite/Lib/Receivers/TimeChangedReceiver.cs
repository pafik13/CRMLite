using System;
using System.Diagnostics;

using Android.App;
using Android.Content;

namespace CRMLite.Receivers
{
	[BroadcastReceiver(Enabled = true)]
	[IntentFilter(new[] { Intent.ActionTimeChanged })]
	public class TimeChangedReceiver : BroadcastReceiver
	{
		public const string C_IS_TIME_CHANGED = "C_IS_TIME_CHANGED";

		public override void OnReceive(Context context, Intent intent)
		{
			Debug.WriteLine(string.Format("TimeChangedReceiver.OnReceive: {0}", DateTimeOffset.Now));
			var isBigDiff = Math.Abs((Helper.GetServerTime() - DateTime.Now).TotalHours) > 1.0d;
			context.GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private)
			       .Edit()
				   .PutBoolean(C_IS_TIME_CHANGED, isBigDiff)
				   .Commit();
			//throw new NotImplementedException();
		}
	}
}

