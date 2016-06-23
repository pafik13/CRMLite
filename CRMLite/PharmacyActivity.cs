
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Realms;

using CRMLite.Entities;

namespace CRMLite
{
	[Activity(Label = "PharmacyActivity")]
	public class PharmacyActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Pharmacy);

			var close = FindViewById<Button>(Resource.Id.paCloseB);
			close.Click += delegate {
				Finish();
			};

			var save = FindViewById<Button>(Resource.Id.paSaveB);
			save.Click += delegate {
				//Toast.MakeText(this, @"Save button clicked", ToastLength.Short).Show();
				// create realm pointing to default file
				using (var realm = Realm.GetInstance())
				{
					// generate test data
					using (var transaction = realm.BeginWrite())
					{
						Pharmacy pharmacy = realm.CreateObject<Pharmacy>();
						pharmacy.UUID = Guid.NewGuid().ToString();
						pharmacy.Name = FindViewById<EditText>(Resource.Id.paNameET).Text;
						pharmacy.LegalName = FindViewById<EditText>(Resource.Id.paLegalNameET).Text;
						pharmacy.Address = FindViewById<EditText>(Resource.Id.paAddressET).Text;
						pharmacy.Subway = FindViewById<EditText>(Resource.Id.paSubwayET).Text;

						transaction.Commit();
					}
				}
				Finish();
			};
				
		}
	}
}

