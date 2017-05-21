using System;
using System.Linq;
using System.Threading.Tasks;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;

using Realms;

using CRMLite.Entities;
using HockeyApp.Android;

namespace CRMLite
{
	//https://forums.xamarin.com/discussion/19362/xamarin-forms-splashscreen-in-android
	[Activity(Label = "CRMLite", Theme = "@style/MyTheme.Splash", Icon = "@mipmap/icon", MainLauncher = true, NoHistory = true)]
	public class SplashActivity : Activity
	{
		public const int C_DB_CURRENT_VERSION = 2;
		ProgressDialog ProgressDialog;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RequestWindowFeature (WindowFeatures.NoTitle);
			Window.AddFlags (WindowManagerFlags.KeepScreenOn);

			CrashManager.Register(this, Secret.HockeyappAppId, new MyCrashManagerListener { ContextHolder = new WeakReference<Context>(this) });

			var mainSharedPreferences = GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private);
			mainSharedPreferences.Edit()
			                     .PutString(MainActivity.C_DUMMY, string.Empty)
			                     .Commit();

			GetSharedPreferences(FilterDialog.C_FILTER_PREFS, FileCreationMode.Private).Edit()
			                                                                           .PutString(MainActivity.C_DUMMY, string.Empty)
			                                                                           .Commit();

			string username = mainSharedPreferences.GetString(Dialogs.SigninDialog.C_USERNAME, string.Empty);

			if (string.IsNullOrEmpty(username)) {
				StartActivity(new Intent(this, typeof(MainActivity)));
				return;
			}

			Task.Factory
			    .StartNew(() => {

					string locFileLocation = System.IO.Path.Combine(
						System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
						username,
						Helper.C_LOC_FILE_NAME
					);

					new System.IO.FileInfo(locFileLocation).Directory.Create();

					var config = new RealmConfiguration(locFileLocation, false) {
						SchemaVersion = C_DB_CURRENT_VERSION,
						MigrationCallback = MigrationCallback_LocDB
					};
					Realm.GetInstance(config);

				})
			    .ContinueWith(task => {
					if (task.IsFaulted || task.IsCanceled) {
						RunOnUiThread(() => {
							Toast.MakeText(this, "Не удалось обновить ГЕОБАЗУ.", ToastLength.Long).Show();
						});
						throw new Exception();
					}

					string dbFileLocation = System.IO.Path.Combine(
						System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
						username,
						Helper.C_DB_FILE_NAME
					);

					new System.IO.FileInfo(dbFileLocation).Directory.Create();

					var config = new RealmConfiguration(dbFileLocation, false) {
						SchemaVersion = C_DB_CURRENT_VERSION,
						MigrationCallback = MigrationCallback_MainDB
					};
					Realm.GetInstance(config);

				})
			    .ContinueWith(task => {
					RunOnUiThread(() => {
						if (task.IsFaulted || task.IsCanceled) {
							Toast.MakeText(this, "Обновление баз не удалось. Напишите адмистратору!", ToastLength.Long).Show();
							Finish();
						} else {
							StartActivity(new Intent(this, typeof(MainActivity)));
						}
					});
				});
		}


		protected void MigrationCallback_MainDB(Migration migration, ulong oldSchemaVersion)
		{
			int count = 0;

			RunOnUiThread(() => {
				ProgressDialog = ProgressDialog.Show(this, "Начинается обновление базы данных", "База данных будет обновлена...", true);
			});

			if (oldSchemaVersion < 1) {
				RunOnUiThread(() => {
					ProgressDialog.SetTitle("База данных обновляется до версии 1");
					ProgressDialog.SetMessage("Обновляются объекты <PhotoData>");
				});

				var newPhotoDatas = migration.NewRealm.All<PhotoData>();
				var oldPhotoDatas = migration.OldRealm.All("PhotoData");

				count = newPhotoDatas.Count();
				for (var i = 0; i < count; i++) {
					var oldPD = oldPhotoDatas.ElementAt(i);
					var newPD = newPhotoDatas.ElementAt(i);

					if (newPD.IsSynced) {
						newPD.ETag = "DummyS3ETag";
						newPD.Bucket = "DummyS3Bucket";
						newPD.Key = "DummyS3Key";
						newPD.Location = "DummyS3Location";
					}
				}
			}

			if (oldSchemaVersion < 2) {
				RunOnUiThread(() => {
					ProgressDialog.SetTitle("База данных обновляется до версии 2");
					ProgressDialog.SetMessage("Обновляются объекты <Agent>");
				});

				var newAgents = migration.NewRealm.All<Agent>();

				count = newAgents.Count();
				for (var i = 0; i < count; i++) {
					var newA = newAgents.ElementAt(i);
					newA.materialType = MaterialType.mtPharmaciesOnly.ToString();
				}
			}
		}

		protected void MigrationCallback_LocDB(Migration migration, ulong oldSchemaVersion)
		{
			RunOnUiThread(() => {
				ProgressDialog = ProgressDialog.Show(this, "Начинается обновление ГЕОБазы", "ГЕОБаза будет обновлена...", true);
			});

			if (oldSchemaVersion < 1) {
			}

			if (oldSchemaVersion < 2) {
			}
		}
	}
}