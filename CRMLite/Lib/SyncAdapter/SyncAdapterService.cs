using Android.OS;
using Android.App;
using Android.Content;

namespace CRMLite.Lib.Sync
{
	[Service(Name = "ru.sbl.crmlite2.SyncAdapterService", Exported = true)] //, Process = ":sync"
	[IntentFilter(new[] { "android.content.SyncAdapter" })]
	[MetaData("android.content.SyncAdapter", Resource = "@xml/syncadapter")]
	public class SyncAdapterService : Service
	{
		static SyncAdapter SyncAdapter;

		static object SyncAdapterLock = new object();

		public override void OnCreate()
		{
			base.OnCreate();

			lock (SyncAdapterLock) {
				if (SyncAdapter == null) {
					SyncAdapter = new SyncAdapter(ApplicationContext, true);
				}
			}
		}

		public override IBinder OnBind(Intent intent)
		{
			return SyncAdapter.SyncAdapterBinder;
		}
	}
}

