using Android.OS;
using Android.App;
using Android.Content;

namespace CRMLite.Lib.Sync
{
	[Service(Name = "ru.sbl.crmlite2.SyncAuthenticatorService")]
	[IntentFilter(new[] { "android.accounts.AccountAuthenticator" })]
	[MetaData("android.accounts.AccountAuthenticator", Resource = "@xml/authenticator")]
	public class SyncAuthenticatorService : Service
	{
		SyncAuthenticator SyncAuthenticator;

		public override void OnCreate()
		{
			base.OnCreate();
			SyncAuthenticator = new SyncAuthenticator(this);
		}

		public override IBinder OnBind(Intent intent)
		{
			//throw new NotImplementedException();
			return SyncAuthenticator.IBinder;
		}
	}
}

