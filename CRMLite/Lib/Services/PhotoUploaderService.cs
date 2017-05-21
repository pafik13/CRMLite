using System;
using System.Linq;
using System.Threading.Tasks;

using Android.OS;
using Android.App;
using Android.Util;
using Android.Content;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using HockeyApp.Android;

using Realms;
using CRMLite.Dialogs;
using CRMLite.Entities;

namespace CRMLite.Services
{
	// https://developer.xamarin.com/api/type/Android.App.Service/
	// TODO: OnLowMemory() This is called when the overall system is running low on memory, and actively running processes should trim their memory usage.
	// TODO: OnTrimMemory(TrimMemory) Called when the operating system has determined that it is a good time for a process to trim unneeded memory from its process.
	[Service(Name = "ru.sbl.crmlite2.PhotoUploaderService", Process = ":photoUploader")] //, Process = ":photoUploader"
	public class PhotoUploaderService : Service
	{
		const string TAG = "ru.sbl.crmlite2.PhotoUploaderService";
		const int NOTIFICATION_ID = Resource.String.foreground_service_photo_uploader;

		#if DEBUG
		const string S3BucketName = "sbl-test1";
		#else
		const string S3BucketName = "sbl-crm-frankfurt";
		#endif

		readonly RegionEndpoint S3Endpoint = RegionEndpoint.EUCentral1;
		const string S3ContentType = "image/jpeg";
		const string S3EndpointLink = "https://s3.eu-central-1.amazonaws.com";

		string DBPath = string.Empty;
		string AgentUUID = string.Empty;
		IAmazonS3 S3Client;

		NotificationManager NotificationManager;
		Task Work;

		public override IBinder OnBind(Intent intent)
		{
			return null;
		}

		public override void OnCreate()
		{
			Log.Info(TAG, "OnCreate");
			base.OnCreate();

			#if DEBUG
			var loggingConfig = AWSConfigs.LoggingConfig;
			loggingConfig.LogMetrics = true;
			loggingConfig.LogResponses = ResponseLoggingOption.Always;
			loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
			loggingConfig.LogTo = LoggingOptions.SystemDiagnostics;
			#endif

			AWSConfigsS3.UseSignatureVersion4 = true;

			S3Client = new AmazonS3Client(Secret.AWSAccessKeyId, Secret.AWSSecretKey, S3Endpoint);
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Info(TAG, "OnStartCommand");

			DBPath = intent.GetStringExtra(MainDatabase.C_DB_PATH);
			AgentUUID = intent.GetStringExtra(SigninDialog.C_AGENT_UUID);

			Log.Info(TAG, "AgentUUID:{0}; DBPath:{1}", AgentUUID, DBPath);

			if (string.IsNullOrEmpty(DBPath)) return StartCommandResult.NotSticky;

			var cm = (Android.Net.ConnectivityManager)Application.Context.GetSystemService(ConnectivityService);

			if (!cm.ActiveNetworkInfo.IsConnectedOrConnecting) return StartCommandResult.NotSticky;

			// Register the crash manager before Initializing the trace writer
			CrashManager.Register(BaseContext, Secret.HockeyappAppId);

			NotificationManager = (NotificationManager)Application.Context.GetSystemService(NotificationService);

			if (Work == null || Work.IsCanceled || Work.IsCompleted || Work.IsFaulted) {

				var content = Work == null ? "Первый запуск" :
					string.Format("Статус предыдущего:{0}Id={1}, Status={2}, IsCanceled={3}, IsCompleted={4}, IsFaulted={5}",
					              System.Environment.NewLine, Work.Id, Work.Status, Work.IsCanceled, Work.IsCompleted, Work.IsFaulted
								 );

				Notification notification = new Notification.Builder(this)
															.SetContentTitle("Выгрузка фото")
				                                            .SetContentText(content)
				                                            .SetSmallIcon(Android.Resource.Drawable.IcPopupSync)
															.Build();
				
				notification.Flags = notification.Flags & NotificationFlags.ForegroundService;

				StartForeground(NOTIFICATION_ID, notification);


				Work = Task.Run(() => {
					try {
						var bucket = S3BucketName.ToLowerInvariant();

						var config = new RealmConfiguration(DBPath, false) {
							SchemaVersion = SplashActivity.C_DB_CURRENT_VERSION
						};
						using (var db = Realm.GetInstance(config)) {

							var photoDatas = db.All<PhotoData>().Where(pd => !pd.IsSynced && string.IsNullOrEmpty(pd.ETag));

							notification = new Notification.Builder(this)
														   .SetContentTitle("Выгрузка фото")
							                               .SetContentText(string.Concat("Кол-во фото: ", photoDatas.Count(), " шт."))
														   .SetSmallIcon(Android.Resource.Drawable.IcPopupSync)
														   .Build();
							
							NotificationManager.Notify(NOTIFICATION_ID, notification);

							System.Threading.Thread.Sleep(5000);

							foreach (var photoData in photoDatas) {
								var key = string.Concat(photoData.UUID, ".jpg");

								notification = new Notification.Builder(this)
															   .SetContentTitle("Выгрузка фото")
															   .SetContentText(string.Concat("START key = ", key))
															   .SetSmallIcon(Android.Resource.Drawable.IcPopupSync)
															   .Build();

								NotificationManager.Notify(NOTIFICATION_ID, notification);

								var response = S3Client.PutObjectAsync(
									new PutObjectRequest {
										BucketName = bucket,
										FilePath = photoData.PhotoPath,
										Key = key,
										CannedACL = S3CannedACL.PublicRead,
										ContentType = S3ContentType
									}).Result;
								//Log.Info(TAG, "ETag:{1}", response.ETag);
								using (var trans = db.BeginWrite()) {
									photoData.Bucket = bucket;
									photoData.ETag = response.ETag;
									photoData.Key = key;
									photoData.Location = string.Format("{0}/{1}/{2}", S3EndpointLink, bucket, key);
									trans.Commit();
								}

								notification = new Notification.Builder(this)
															   .SetContentTitle("Выгрузка фото")
															   .SetContentText(string.Concat("END key = ", key))
															   .SetSmallIcon(Android.Resource.Drawable.IcPopupSync)
															   .Build();

								NotificationManager.Notify(NOTIFICATION_ID, notification);
							}
						}
					} catch (Exception ex) {
						var exeptionNotification = new Notification.Builder(this)
						                                           .SetContentTitle("Выгрузка фото")
						                                           .SetContentText(ex.Message)
						                                           .SetSmallIcon(Android.Resource.Drawable.IcPopupSync)
						                                           .Build();
						exeptionNotification.Flags = exeptionNotification.Flags & NotificationFlags.ForegroundService;

						NotificationManager.Notify(NOTIFICATION_ID, exeptionNotification);

						System.Threading.Thread.Sleep(5000);
					} finally {
						StopForeground(true);
						StopSelf();
					}
				});
			}

			return StartCommandResult.Sticky;
		}

		public override void OnDestroy()
		{
			Log.Info(TAG, "OnDestroy");
			StopForeground(true);

			base.OnDestroy();
		}
	}
}

