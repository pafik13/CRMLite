using System;
using System.Linq;
using System.Threading.Tasks;

using SD = System.Diagnostics;

using Android.OS;
using Android.App;
using Android.Util;
using Android.Content;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using Realms;

using CRMLite;
using CRMLite.Dialogs;
using CRMLite.Entities;

namespace CRMLite.Services
{
	[Service(Name = "ru.sbl.crmlite2.PhotoUploaderService", Process = ":photoUploader")] //, Process = ":photoUploader"
	public class PhotoUploaderService : Service
	{
		const string TAG = "ru.sbl.crmlite2.PhotoUploaderService";
		#if DEBUG
		//const string S3BucketName = "sbl-test1";
		const string S3BucketName = "sbl-crm-frankfurt";
		#else
		const string S3BucketName = "sbl-crm-frankfurt";
		#endif

		readonly RegionEndpoint S3Endpoint = RegionEndpoint.EUCentral1;
		const string S3ContentType = "image/jpeg";
		const string S3EndpointLink = "https://s3.eu-central-1.amazonaws.com";

		string DBPath = string.Empty;
		string AgentUUID = string.Empty;
		IAmazonS3 S3Client;

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

			var wifiService = BaseContext.GetSystemService(WifiService);

			if (Work == null || Work.IsCanceled || Work.IsCompleted || Work.IsFaulted) {
				Work = Task.Run(() => {
					var config = new RealmConfiguration(DBPath, false) {
						SchemaVersion = 1
					};

					var realm = Realm.GetInstance(config);

					var bucket = S3BucketName.ToLowerInvariant();
					var photoDatas = realm.All<PhotoData>().Where(pd => !pd.IsSynced && string.IsNullOrEmpty(pd.ETag));
					foreach (var photoData in photoDatas) {
						var key = string.Concat(photoData.UUID, ".jpg");
						var response = S3Client.PutObjectAsync(
							new PutObjectRequest {
								BucketName = bucket,
								FilePath = photoData.PhotoPath,
								Key = key,
								CannedACL = S3CannedACL.PublicRead,
								ContentType = S3ContentType
							}).Result;
						//Log.Info(TAG, "ETag:{1}", response.ETag);
						using (var trans = realm.BeginWrite()) {
							photoData.Bucket = bucket;
							photoData.ETag = response.ETag;
							photoData.Key = key;
							photoData.Location = string.Format("{0}/{1}/{2}", S3EndpointLink, bucket, key);
							trans.Commit();
						}
					}

					StopSelf();
				});
			}

			return StartCommandResult.Sticky;
		}

		public override void OnDestroy()
		{
			Log.Info(TAG, "OnDestroy");
			base.OnDestroy();
		}
	}
}

