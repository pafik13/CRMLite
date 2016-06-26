using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using Realms;
using Newtonsoft.Json;

using CRMLite.Entities;
using CRMLite.Suggestions;
using System.Linq;

namespace CRMLite.Dialogs
{
	public class HospitalDialog : DialogFragment
	{

		public event EventHandler AfterSaved;

		Activity context = null;
		Pharmacy pharmacy = null;
		Hospital hospital = null;

		Transaction transaction = null;

		protected virtual void OnAfterSaved(EventArgs e)
		{
			if (AfterSaved != null)
			{
				AfterSaved(this, e);
			}
		}

		public HospitalDialog(Activity context, Pharmacy pharmacy, Hospital hospital = null)
		{
			if (context == null || pharmacy == null) {
				throw new ArgumentNullException();
			}
			this.context = context;
			this.pharmacy = pharmacy;
			this.hospital = hospital;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			Dialog.SetCanceledOnTouchOutside(false);

			transaction = MainDatabase.BeginTransaction();

			var caption = string.Empty;
			if (hospital == null)
			{
				//Dialog.SetTitle("НОВЫЙ СОТРУДНИК");
				caption = "НОВОЕ ЛПУ";
				hospital = MainDatabase.CreateHospital(pharmacy.UUID);
				hospital.Address = "Москва";
			}
			else
			{
				//Dialog.SetTitle("СОТРУДНИК : " + employee.Name);
				caption = "ЛПУ : " + hospital.Name;

				if (hospital.LastSyncResult != null)
				{
					caption += string.Format(" (синхр. {0} в {1})"
											 , hospital.LastSyncResult.createdAt.ToLocalTime().ToString("dd.MM.yy")
											 , hospital.LastSyncResult.createdAt.ToLocalTime().ToString("HH:mm:ss")
											);
				}
			}

			Dialog.SetTitle(caption);

			View view = inflater.Inflate(Resource.Layout.HospitalDialog, container, false);

			view.FindViewById<EditText>(Resource.Id.hdNameET).Text = hospital.Name;


			var token = Secret.DadataApiToken;
			var url = "https://suggestions.dadata.ru/suggestions/api/4_1/rs";
			var api = new SuggestClient(token, url);
			AutoCompleteTextView text = view.FindViewById<AutoCompleteTextView>(Resource.Id.hdAddressACTV);
			text.Text = hospital.Address;
			text.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) =>
			{
				if (text.Text.Contains(" "))
				{
					var response = api.QueryAddress(text.Text);
					var suggestions = response.suggestionss.Select(x => x.value).ToArray();
					text.Adapter = new ArrayAdapter<string>(context, Android.Resource.Layout.SimpleDropDownItem1Line, suggestions); ;
					(text.Adapter as ArrayAdapter<string>).NotifyDataSetChanged();
					if (text.IsShown)
					{
						text.DismissDropDown();
					}
					text.ShowDropDown();
				}
			};

			view.FindViewById<Button>(Resource.Id.hdCloseB).Click += delegate
			{
				if (hospital.CreatedAt == null)
				{
					MainDatabase.DeleteHospital(hospital);
				}

				transaction.Commit();

				Dismiss();
			};

			view.FindViewById<Button>(Resource.Id.hdSaveB).Click += delegate
			{
				//Toast.MakeText(context, "SAVE BUTTON CLICKED", ToastLength.Short).Show();
				hospital.CreatedAt = DateTimeOffset.Now;
				hospital.Name = view.FindViewById<EditText>(Resource.Id.hdNameET).Text;
				hospital.Address = view.FindViewById<EditText>(Resource.Id.hdAddressACTV).Text;

				transaction.Commit();

				var sync = new SyncItem()
				{
					Path = @"Hospital",
					ObjectUUID = hospital.UUID,
					JSON = JsonConvert.SerializeObject(hospital)
				};

				MainDatabase.AddToQueue(sync);

				context.StartService(new Intent("com.xamarin.SyncService"));

				OnAfterSaved(EventArgs.Empty);

				Dismiss();
			};

			return view;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			context = null;
			pharmacy = null;
			hospital = null;
		}
	}
}

