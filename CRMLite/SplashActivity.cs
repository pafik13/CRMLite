using System.Linq;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Content;

using Realms;

using CRMLite.Entities;

namespace CRMLite
{
	//https://forums.xamarin.com/discussion/19362/xamarin-forms-splashscreen-in-android
	[Activity(Label = "CRMLite", Theme = "@style/MyTheme.Splash", Icon = "@mipmap/icon", MainLauncher = true, NoHistory = true)]
	public class SplashActivity : Activity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			RequestWindowFeature (WindowFeatures.NoTitle);
			Window.AddFlags (WindowManagerFlags.KeepScreenOn);

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

			string dbFileLocation = System.IO.Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
				username,
				Helper.C_DB_FILE_NAME
			);
			new System.IO.FileInfo(dbFileLocation).Directory.Create();
			var config = new RealmConfiguration(dbFileLocation, false) {
				SchemaVersion = 5,
				MigrationCallback = MigrationCallback
			};
			Realm.GetInstance(config);

			StartActivity(new Intent(this, typeof(MainActivity)));
		}

		protected void MigrationCallback(Migration migration, ulong oldSchemaVersion)
		{
			int count = 0;

			//var progressDialog = new ProgressDialog(this);
			//progressDialog.SetTitle("Начинается обновление базы данных");
			//progressDialog.SetMessage("База данных будет обновлена...");
			var progressDialog = ProgressDialog.Show(this, "Начинается обновление базы данных", "База данных будет обновлена...", true);

			progressDialog.Progress = 0;
			if (oldSchemaVersion < 1) {
				progressDialog.SetTitle("База данных обновляется до версии 1");
				var newPhotoDatas = migration.NewRealm.All<PhotoData>();
				var oldPhotoDatas = migration.OldRealm.All("PhotoData");

				count = newPhotoDatas.Count();
				progressDialog.Max = count;
				for (var i = 0; i < count; i++) {
					var oldPD = oldPhotoDatas.ElementAt(i);
					var newPD = newPhotoDatas.ElementAt(i);

					if (newPD.IsSynced) {
						newPD.ETag = "DummyS3ETag";
						newPD.Bucket = "DummyS3Bucket";
						newPD.Key = "DummyS3Key";
						newPD.Location = "DummyS3Location";
					}
					progressDialog.Progress = i + 1;
				}
			}

			progressDialog.Progress = 0;
			if (oldSchemaVersion < 2) {
				progressDialog.SetTitle("База данных обновляется до версии 2");

				progressDialog.SetMessage("Обновляется <Agent>");
				var newAgents = migration.NewRealm.All<Agent>();
				var oldAgents = migration.OldRealm.All("Agent");

				count = newAgents.Count();
				progressDialog.Max = count;
				for (var i = 0; i < count; i++) {
					var oldA = oldAgents.ElementAt(i) as Agent;
					var newA = newAgents.ElementAt(i);

					newA.isDummyBool = oldA.GetSex() == Sex.Female;

					progressDialog.Progress = i + 1;
				}

				progressDialog.SetMessage("Обновляется <DistributionData>");
				var newDDs = migration.NewRealm.All<DistributionData>();
				var oldDDs = migration.OldRealm.All("DistributionData");

				count = newDDs.Count();
				progressDialog.Max = count;
				for (var i = 0; i < count; i++) {
					var oldDD = oldDDs.ElementAt(i) as DistributionData;
					var newDD = newDDs.ElementAt(i);

					newDD.isDummyBool = oldDD.HasPOS && oldDD.IsExistence && oldDD.IsManaged;

					progressDialog.Progress = i + 1;
				}
			}

			progressDialog.Progress = 0;
			if (oldSchemaVersion < 3) {
				progressDialog.SetTitle("База данных обновляется до версии 2");

				progressDialog.SetMessage("Обновляется <Agent>");
				var newAgents = migration.NewRealm.All<Agent>();
				var oldAgents = migration.OldRealm.All("Agent");

				count = newAgents.Count();
				progressDialog.Max = count;
				for (var i = 0; i < count; i++) {
					var oldA = oldAgents.ElementAt(i) as Agent;
					var newA = newAgents.ElementAt(i);

					newA.isDummyBool = oldA.GetSex() == Sex.Female;

					progressDialog.Progress = i + 1;
				}

				progressDialog.SetMessage("Обновляется <DistributionData>");
				var newDDs = migration.NewRealm.All<DistributionData>();
				var oldDDs = migration.OldRealm.All("DistributionData");

				count = newDDs.Count();
				progressDialog.Max = count;
				for (var i = 0; i < count; i++) {
					var oldDD = oldDDs.ElementAt(i) as DistributionData;
					var newDD = newDDs.ElementAt(i);

					newDD.isDummyBool = oldDD.HasPOS && oldDD.IsExistence && oldDD.IsManaged;

					progressDialog.Progress = i + 1;
				}
			}

			progressDialog.Progress = 0;
			if (oldSchemaVersion < 5) {
				progressDialog.SetTitle("База данных обновляется до версии 2");

				progressDialog.SetMessage("Обновляется <Agent>");
				progressDialog.Max = 10;

				for (int i = 0; i < 100; i++) {
					progressDialog.Progress = i * 10 / 100;
					System.Threading.Thread.Sleep(100);
				}
			}
		}


	}
}