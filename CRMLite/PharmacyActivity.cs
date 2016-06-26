using System;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using Realms;

using Newtonsoft.Json;

using CRMLite.Entities;
using CRMLite.Suggestions;

namespace CRMLite
{
	[Activity(Label = "PharmacyActivity")]
	public class PharmacyActivity : Activity
	{
		Pharmacy pharmacy;
		Transaction transaction;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Pharmacy);

			transaction = MainDatabase.BeginTransaction();

			var pharmacyUUID = Intent.GetStringExtra("UUID");
			var caption = "ДОБАВЛЕНИЕ НОВОЙ АПТЕКИ";
			if (string.IsNullOrEmpty(pharmacyUUID))
			{
				pharmacy = MainDatabase.CreatePharmacy();
				pharmacy.Address = "Москва";
			}
			else
			{
				pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
				var result = MainDatabase.GetSyncResult(pharmacyUUID);
				if (result != null)
				{
					pharmacy.LastSyncResult = result;
				}
				caption = "АПТЕКА : " + pharmacy.LegalName;

				if (pharmacy.LastSyncResult != null)
				{
					caption += string.Format(" (синхр. {0} в {1})"
					                         , pharmacy.LastSyncResult.createdAt.ToLocalTime().ToString("dd.MM.yy")
					                         , pharmacy.LastSyncResult.createdAt.ToLocalTime().ToString("HH:mm:ss")
					                        );
				}
			}

			FindViewById<TextView>(Resource.Id.paUUIDTV).Text = pharmacy.UUID;
			FindViewById<TextView>(Resource.Id.paInfoTV).Text = caption;
			FindViewById<EditText>(Resource.Id.paNameET).Text = pharmacy.Name;
			FindViewById<EditText>(Resource.Id.paLegalNameET).Text = pharmacy.LegalName;
			FindViewById<AutoCompleteTextView>(Resource.Id.paAddressACTV).Text = pharmacy.Address;
			FindViewById<EditText>(Resource.Id.paSubwayACTV).Text = pharmacy.Subway;
			FindViewById<EditText>(Resource.Id.paPhoneET).Text = pharmacy.Phone;


			var close = FindViewById<Button>(Resource.Id.paCloseB);
			close.Click += delegate {
				if (pharmacy.CreatedAt == null)
				{
					MainDatabase.DeletePharmacy(pharmacy);
				}

				transaction.Commit();
				Finish();
			};

			var save = FindViewById<Button>(Resource.Id.paSaveB);
			save.Click += delegate {
				pharmacy.CreatedAt = DateTimeOffset.Now;
				pharmacy.Name = FindViewById<EditText>(Resource.Id.paNameET).Text;
				pharmacy.LegalName = FindViewById<EditText>(Resource.Id.paLegalNameET).Text;
				pharmacy.Address = FindViewById<EditText>(Resource.Id.paAddressACTV).Text;
				pharmacy.Subway = FindViewById<EditText>(Resource.Id.paSubwayACTV).Text;
				pharmacy.Phone = FindViewById<EditText>(Resource.Id.paPhoneET).Text;

				transaction.Commit();

				var sync = new SyncItem()
				{
					Path = @"Pharmacy",
					ObjectUUID = pharmacy.UUID,
					JSON = JsonConvert.SerializeObject(pharmacy)
				};

				MainDatabase.AddToQueue(sync);

				StartService(new Intent("com.xamarin.SyncService"));
				//Intent intent = new Intent("com.xamarin.SyncService");
				//intent.PutExtra(SyncService.C_UUID, pharmacy.UUID);
				//intent.PutExtra(SyncService.C_JSON, JsonConvert.SerializeObject(pharmacy));
				//StartService(intent);

				Finish();
			};

			var token = Secret.DadataApiToken;
			var url = "https://suggestions.dadata.ru/suggestions/api/4_1/rs";
			var api = new SuggestClient(token, url);
			AutoCompleteTextView text = FindViewById<AutoCompleteTextView>(Resource.Id.paAddressACTV);
			text.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) =>
			{
				if (text.Text.Contains(" "))
				{
					var response = api.QueryAddress(text.Text);
					var suggestions = response.suggestionss.Select(x => x.value).ToArray();
					text.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, suggestions); ;
					(text.Adapter as ArrayAdapter<String>).NotifyDataSetChanged();
					if (text.IsShown)
					{
						text.DismissDropDown();
					}
					text.ShowDropDown();
				}
			};


			AutoCompleteTextView subway = FindViewById<AutoCompleteTextView>(Resource.Id.paSubwayACTV);
			string[] stations = Resources.GetStringArray(Resource.Array.moscow_stations);
			subway.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, stations);
		}
	}
}

