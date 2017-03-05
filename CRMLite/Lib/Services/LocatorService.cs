using System;

using Android.App;
using Android.Util;
using Android.Content;
using Android.OS;
using Android.Locations;

using Realms;

using CRMLite.Entities;

namespace CRMLite.Services
{
	[Service(Name = "ru.sbl.crmlite2.LocatorService", Process = ":locator")] //, Process = ":locator", Exported=true
	public class LocatorService: Service, ILocationListener
	{
		const string TAG = "ru.sbl.crmlite2.LocatorService";
		const int LOCATION_INTERVAL = 5000;
		const float LOCATION_DISTANCE = 0f;

		LocationManager LocationManager;
		string LOCPath = string.Empty;
		Realm Realm;

		public override IBinder OnBind(Intent intent)
		{
			return null;
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Info(TAG, "OnStartCommand");

			LOCPath = intent.GetStringExtra(MainDatabase.C_LOC_PATH);

			Log.Info(TAG, "LOCPath:{0}", LOCPath);

			if (string.IsNullOrEmpty(LOCPath)) return StartCommandResult.NotSticky;

			Notification notification = new Notification.Builder(this)
														.SetContentTitle("Местоположение")
														.SetContentText("Определение местоположения!")
														.SetSmallIcon(Android.Resource.Drawable.IcDialogMap)
														.Build();

			StartForeground((int)NotificationFlags.ForegroundService, notification);

			LocationManager = (LocationManager)Application.Context.GetSystemService(LocationService);

			if (LocationManager != null) {
				LocationManager.RequestLocationUpdates(
					LocationManager.NetworkProvider, LOCATION_INTERVAL, LOCATION_DISTANCE, this
				);
				var loc = LocationManager.GetLastKnownLocation(LocationManager.NetworkProvider);

				Realm = Realm.GetInstance(LOCPath);
			}

			return StartCommandResult.Sticky;
		}

		public override void OnDestroy()
		{
			Log.Info(TAG, "OnDestroy");

			base.OnDestroy();
			LocationManager.RemoveUpdates(this);
			StopForeground(true);
			Realm.Close();
		}

		public void OnLocationChanged(Location location)
		{
			Log.Info(TAG, "OnLocationChanged");

			if (Realm == null) {
				Realm = Realm.GetInstance(LOCPath);
			}

			Realm.Write(() => {
				var loc = Realm.CreateObject<GPSLocation>();
				loc.CreatedAt = DateTimeOffset.Now;
				loc.UpdatedAt = DateTimeOffset.Now;
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

			Log.Info(TAG, "OnLocationChanged: GPSLocation={0}", Realm.All<GPSLocation>().Count());
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

