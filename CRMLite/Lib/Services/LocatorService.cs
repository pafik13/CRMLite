using System;

using Android.App;
using Android.Util;
using Android.Content;
using Android.OS;
using Android.Locations;

using HockeyApp.Android;

using Realms;

using CRMLite.Dialogs;
using CRMLite.Entities;
using System.Threading.Tasks;

namespace CRMLite.Services
{
	// https://developer.xamarin.com/api/type/Android.App.Service/
	// TODO: OnLowMemory() This is called when the overall system is running low on memory, and actively running processes should trim their memory usage.
	// TODO: OnTrimMemory(TrimMemory) Called when the operating system has determined that it is a good time for a process to trim unneeded memory from its process.
	[Service(Name = "ru.sbl.crmlite2.LocatorService", Process = ":locator")] //, Process = ":locator", Exported=true
	public class LocatorService: Service, ILocationListener
	{
		const string TAG = "ru.sbl.crmlite2.LocatorService";
		const int NOTIFICATION_ID = Resource.String.foreground_service_locator;

		const int INTERVAL_SEC = 1000;
		const int INTERVAL_MIN = 60 * INTERVAL_SEC;
		const int LOCATION_INTERVAL_NET = 5 * INTERVAL_SEC;
		const int LOCATION_INTERVAL_GPS = 30 * INTERVAL_SEC;
		const int IDLE_INTERVAL = 20 * INTERVAL_MIN;
		const float LOCATION_DISTANCE = 0f;

		LocationManager LocationManager;
		string LOCPath = string.Empty;
		string AgentUUID = string.Empty;
		DateTimeOffset LastCallTime = DateTimeOffset.MinValue;
		bool IsLocatorNetRequestOn;
		bool IsLocatorGPSRequestOn;
		int LocatorNetRequestPeriod;
		int LocatorGPSRequestPeriod;
		int LocatorIdlePeriod;
		
		RealmConfiguration DBConfig;
		
		public override IBinder OnBind(Intent intent)
		{
			return null;
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Info(TAG, "OnStartCommand");

			LastCallTime = DateTimeOffset.Now;
			LOCPath = intent.GetStringExtra(MainDatabase.C_LOC_PATH);
			AgentUUID = intent.GetStringExtra(SigninDialog.C_AGENT_UUID);

			Log.Info(TAG, "AgentUUID:{0}; LOCPath:{1}", AgentUUID, LOCPath);

			if (string.IsNullOrEmpty(LOCPath)) return StartCommandResult.NotSticky;

			// Register the crash manager before Initializing the trace writer
			CrashManager.Register(BaseContext, Secret.HockeyappAppId);

			IsLocatorNetRequestOn = intent.GetBooleanExtra(Customizations.IsLocatorNetRequestOn, true);
			IsLocatorGPSRequestOn = intent.GetBooleanExtra(Customizations.IsLocatorGPSRequestOn, true);

			Log.Info(TAG, "IsLocatorNetRequestOn:{0}; IsLocatorGPSRequestOn:{1}", IsLocatorNetRequestOn, IsLocatorGPSRequestOn);
			if (!(IsLocatorNetRequestOn || IsLocatorGPSRequestOn)) return StartCommandResult.NotSticky;

			LocatorNetRequestPeriod = intent.GetIntExtra(Customizations.LocatorNetRequestPeriod, LOCATION_INTERVAL_NET);
			LocatorGPSRequestPeriod = intent.GetIntExtra(Customizations.LocatorGPSRequestPeriod, LOCATION_INTERVAL_GPS);
			LocatorIdlePeriod = intent.GetIntExtra(Customizations.LocatorIdlePeriod, IDLE_INTERVAL);

			Log.Info(TAG, "LocatorNetRequestPeriod:{0}; LocatorGPSRequestPeriod:{1}; LocatorIdlePeriod:{2}", 
			         LocatorNetRequestPeriod, LocatorGPSRequestPeriod, LocatorIdlePeriod
			        );

			Notification notification = new Notification.Builder(this)
														.SetContentTitle("Местоположение")
														.SetContentText("Непрерывное определение местоположения!")
														.SetSmallIcon(Android.Resource.Drawable.IcDialogMap)
														.Build();

			StartForeground(NOTIFICATION_ID, notification);

			LocationManager = (LocationManager)Application.Context.GetSystemService(LocationService);

			if (LocationManager != null) {
				if (IsLocatorNetRequestOn) {
					LocationManager.RequestLocationUpdates(
						LocationManager.NetworkProvider, LocatorNetRequestPeriod, LOCATION_DISTANCE, this
					);
					var loc = LocationManager.GetLastKnownLocation(LocationManager.NetworkProvider);
					if (loc == null) {
						Log.Info(TAG, "Provider:Network, Location:Null");
					} else {
						Log.Info(TAG, "Provider:{0}, Latitude:{1}, Longitude:{2}", loc.Provider, loc.Latitude, loc.Longitude);
					}
				}

				if (IsLocatorGPSRequestOn) {
					LocationManager.RequestLocationUpdates(
						LocationManager.GpsProvider, LocatorGPSRequestPeriod, LOCATION_DISTANCE, this
					);
					var loc = LocationManager.GetLastKnownLocation(LocationManager.GpsProvider);
					if (loc == null) {
						Log.Info(TAG, "Provider:GPS, Location:Null");
					} else {
						Log.Info(TAG, "Provider:{0}, Latitude:{1}, Longitude:{2}", loc.Provider, loc.Latitude, loc.Longitude);
					}				
				}

				DBConfig = new RealmConfiguration(LOCPath) {
					SchemaVersion = SplashActivity.C_DB_CURRENT_VERSION
				};
			}

			Task.Delay(LocatorIdlePeriod)
			    .ContinueWith((Task task) => {
					StopForeground(true);
					StopSelf();
				});

			return StartCommandResult.Sticky;
		}

		public override void OnDestroy()
		{
			Log.Info(TAG, "OnDestroy");

			LocationManager.RemoveUpdates(this);
			StopForeground(true);

			base.OnDestroy();
		}

		public void OnLocationChanged(Location location)
		{
			Log.Info(TAG, "OnLocationChanged: Provider={0}", location.Provider);

 			if (DBConfig == null) {
				DBConfig = new RealmConfiguration(LOCPath) {
					SchemaVersion = SplashActivity.C_DB_CURRENT_VERSION
				};
			}
			
			using(var db = Realm.GetInstance(DBConfig)) {
				db.Write(() => {
					var loc = db.CreateObject<GPSLocation>();
					loc.CreatedAt = DateTimeOffset.Now;
					loc.UpdatedAt = DateTimeOffset.Now;
					loc.CreatedBy = AgentUUID;
					loc.UUID = Guid.NewGuid().ToString();
					loc.Accuracy = location.Accuracy;
					loc.Altitude = location.Altitude;
					loc.Bearing = location.Bearing;
					loc.ElapsedRealtimeNanos = location.ElapsedRealtimeNanos;
					loc.IsFromMockProvider = location.IsFromMockProvider;
					loc.Latitude = location.Latitude;
					loc.Longitude = location.Longitude;
					loc.Provider = location.Provider;
					loc.Speed = location.Speed;
					loc.Time = location.Time;
				});

				Log.Info(TAG, "OnLocationChanged: GPSLocation={0}", db.All<GPSLocation>().AsRealmCollection().Count);
			}
		}

		public void OnProviderDisabled(string provider)
		{
			Log.Info(TAG, "OnProviderDisabled: {0}", provider);
		}

		public void OnProviderEnabled(string provider)
		{
			Log.Info(TAG, "OnProviderEnabled: {0}", provider);
		}

		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
			Log.Info(TAG, "OnStatusChanged: {0}-{1}", provider, status);
		}
	}
}

