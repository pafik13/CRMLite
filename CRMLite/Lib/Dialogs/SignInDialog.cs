using System;
using SD = System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

using RestSharp;
using RestSharp.Serializers;
using RestSharp.Authenticators;

using CRMLite.Entities;

namespace CRMLite.Dialogs
{
	public class SigninDialog : DialogFragment
	{
		const char quote = '"';
		public const string C_USERNAME = @"C_USERNAME";
		public const string TAG = @"SigninDialog";

		Button bSignUp = null;

		Animation mAnimation= null;
		Context context = null;
		Activity activity = null;
		LinearLayout llInfo = null;
		LinearLayout llSuccess = null;
		LinearLayout llWarning = null;
		LinearLayout llDanger = null;
		TextView tvProgressInfo = null;
		TextView tvProgressSuccess = null;
		TextView tvProgressWarning = null;
		TextView tvProgressDanger = null;

		TextView tvUsername = null;
		TextView tvPassword = null;

		//User user = new User();

		string cookieName = "";
		string cookieValue = "";

		public event EventHandler SuccessSignedIn;

		public SigninDialog (Activity parent)
		{
			activity = parent;
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			this.RetainInstance = true;
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView (inflater, container, savedInstanceState);

			Dialog.SetCanceledOnTouchOutside (true);

			var view = inflater.Inflate (Resource.Layout.SignInDialog, container, false);

			bSignUp = view.FindViewById<Button> (Resource.Id.btnDialogEmail);

			llInfo = view.FindViewById<LinearLayout> (Resource.Id.llInfo);
			llSuccess = view.FindViewById<LinearLayout> (Resource.Id.llSuccess);
			llWarning = view.FindViewById<LinearLayout> (Resource.Id.llWarning);
			llDanger = view.FindViewById<LinearLayout> (Resource.Id.llDanger);
			tvProgressInfo = view.FindViewById<TextView> (Resource.Id.sidProgressInfo);
			tvProgressSuccess = view.FindViewById<TextView> (Resource.Id.sidProgressSuccess);
			tvProgressWarning = view.FindViewById<TextView> (Resource.Id.sidProgressWarning);
			tvProgressDanger = view.FindViewById<TextView> (Resource.Id.sidProgressDanger);

			tvUsername = view.FindViewById<TextView> (Resource.Id.txtUsername);
			tvPassword = view.FindViewById<TextView> (Resource.Id.txtPassword);

			context = inflater.Context;

			bSignUp.Click += (object sender, EventArgs e) => {
				Toast.MakeText(inflater.Context, @"Click", ToastLength.Short).Show();
				mAnimation = AnimationUtils.LoadAnimation(inflater.Context,Resource.Animation.slide_right);
//				anim.FillAfter = true;
//				anim.AnimationEnd += Anim_AnimationEnd;
//				mAnimation = new TranslateAnimation(
//					Dimension.RelativeToSelf, 0f,
//					Dimension.RelativeToParent, 1.0f,
//					Dimension.Absolute, 0f,
//					Dimension.Absolute, 0f);
//				//mAnimation.FillAfter = true;
//				mAnimation.Duration = 500;
				//mAnimation.RepeatCount = -1;
				//mAnimation.RepeatMode = RepeatMode.Reverse;
				//mAnimation.Interpolator = new LinearInterpolator();
				mAnimation.AnimationEnd += Anim_AnimationEnd;
				bSignUp.StartAnimation(mAnimation);

//				ThreadPool.QueueUserWorkItem (o => SlowMethod ());

				//bSignUp.Animate().TranslationY(bSignUp.Height).SetListener(new ;
//				if (mAnimation.HasEnded) {
//					Toast.MakeText(inflater.Context, @"Animation End", ToastLength.Short).Show();
//				} else {
//					Toast.MakeText(inflater.Context, @"Animation NOT End", ToastLength.Short).Show();
//				}
			};

			return view;
		}

		public override void OnDestroyView()
		{
			if (this.Dialog != null && this.RetainInstance)
				this.Dialog.SetDismissMessage(null);
			base.OnDestroyView();
		}

		protected virtual void OnSuccessSignedIn(EventArgs e)
		{
			if (SuccessSignedIn != null) {
				SuccessSignedIn (this, e);
			}
		}

		private void SlowMethod ()
		{
//			Thread.Sleep (5000);
//
//							if (mAnimation.HasEnded) {
//				activity.RunOnUiThread(() => Toast.MakeText(context, @"Animation End", ToastLength.Short).Show());
//							} else {
//				activity.RunOnUiThread(() =>Toast.MakeText(context, @"Animation NOT End", ToastLength.Short).Show());
//							}

			//user.username = @"Zvezdova957";
			//user.password = @"624590701";

			//user.username = tvUsername.Text;
			//user.password = tvPassword.Text;

			bool isAuth = false;

			if (true) {
				isAuth = onlineAuth(tvUsername.Text, tvPassword.Text);
			} else {
				isAuth = offlineAuth(tvUsername.Text, tvPassword.Text);
			}

			if (isAuth)
			{

				//Toast.MakeText(context, @"Authentificated", ToastLength.Short).Show())

				WriteInfo (@"Обновление внутренних данных", 2000);
				try {
					//Log.Error(@"Login", ex.Message);
					//Common.SetCurrentUser (user);
					//PharmacyManager.Refresh ();
					//AttendanceManager.Refresh ();
					//AttendancePhotoManager.Refresh ();
					//AttendanceResultManager.Refresh ();
					//SyncQueueManager.Refresh ();
				} catch (Exception ex) {
					Log.Error (@"Login", ex.Message);
					WriteDanger (@"ОШИБКА! ВХОД НЕ ВЫПОЛНЕН", 3000);
					return;
				}
				WriteSuccess(@"ВХОД ВЫПОЛНЕН УСПЕШНО", 3000);
				Dismiss ();
				OnSuccessSignedIn (EventArgs.Empty);
				//				MessageBox.Show();
				MainDatabase.Dispose();
				activity.RunOnUiThread(() => MainDatabase.Username = tvUsername.Text);
			}
			else
			{
				activity.RunOnUiThread(() => Toast.MakeText(context, @"NOT Authentificated", ToastLength.Short).Show());
				WriteDanger (@"ОШИБКА! ВХОД НЕ ВЫПОЛНЕН", 3000);
//				activity.RunOnUiThread (() => bSignUp.Visibility = ViewStates.Visible);
//				MessageBox.Show(@"NOT Authentificated");
			}

			//RunOnUiThread (() => textview.Text = "Method Complete");
		}

		bool offlineAuth(string username, string password)
		{
			Log.Info(@"Info", @"Начало проверки данных");
			return false;
//			User user = Common.GetUser(username);

//			if (user == null)
//			{
//				//Debug.WriteLine(String.Format(@"Не найдена информация о пользователе %s. Попробуйте подключить интернет и повторить попытку.", username), @"Info");
//				return false;
//			}
//			else
//			{
//				if (!user.password.Equals(password))
//				{
//					//Debug.WriteLine(@"Не удалось пройти аутентификацию! Проверьте, пожалуйста, логин и пароль.", @"Info");
//					return false;
//				} else
//				{
//					//Debug.WriteLine("Проверка наличия информации себе", @"Info");

//					if (Common.GetMerchant(username) == null)
//					{
//						//Debug.WriteLine(@"Не удалось найти информацию о себе! Попробуйте подключить интернет и повторить попытку.", @"Info");
//						return false;
//					}
//					else
//					{
//						//Debug.WriteLine("Проверка наличия информации о менеджере", @"Info");

//						if (Common.GetManager(username) == null)
//						{
//							//Debug.WriteLine(@"Не удалось найти информацию о менеджере! Попробуйте подключить интернет и повторить попытку.", @"Info");
//							return false;
//						}
//						else
//						{
//							//Debug.WriteLine("Проверка наличия информации о проекте", @"Info");

//							if (Common.GetProject(username) == null)
//							{
//								//Debug.WriteLine(@"Не удалось найти информацию о проекте! Попробуйте подключить интернет и повторить попытку.", @"Info");
//								return false;
//							}
//							else
//							{
//								//Debug.WriteLine("Проверка наличия информации о препаратах", @"Info");

//								if (Common.GetDrugs(username) == null)
//								{
//									//Debug.WriteLine(@"Не удалось найти информацию о препаратах! Попробуйте подключить интернет и повторить попытку.", @"Info");
//									return false;
//								}
//								else
//								{
//									//Debug.WriteLine("Проверка наличия информации о собираемых данных", @"Info");
//
//									if (Common.GetInfos(username) == null)
//									{
//										//Debug.WriteLine(@"Не удалось найти информацию о собираемых данных! Попробуйте подключить интернет и повторить попытку.", @"Info");//
//										return false;
//									}
//									else
//									{
//										//Debug.WriteLine("Проверка наличия информации о района деятельности", @"Info");
//
//										if (Common.GetTerritory(username) == null)
//										{
//											//Debug.WriteLine(@"Не удалось найти информацию о района деятельности! Попробуйте подключить интернет и повторить попытку.", @"Info");//
//											return false;
//										}
//										else
//										{
//											//Debug.WriteLine("Проверка наличия информации об аптеках района деятельности", @"Info");

//											if (Common.GetPharmacies(username) == null)
//											{
//												//Debug.WriteLine(@"Не удалось найти информацию об аптеках района деятельности! Попробуйте подключить интернет и повторить попытку.", @"Info");
//												return false;
//											}
//										}
//									}
//								}
//							}
//						}

//					}

//					return true;
//				}

//			}
		}

		bool onlineAuth(string username, string password)
		{
			WriteInfo (@"Подключение к серверу");

			var client = new RestClient(@"http://front-sblcrm.rhcloud.com/");

			MainDatabase.Username = username;

			Helper.Username = username;
			activity.GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private)
			        .Edit()
			        .PutString(C_USERNAME, username)
			        .Commit();

			WriteInfo (@"Получение LoadPositions", 2000);
			try {
				LoadPositions(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadNets", 2000);
			try {
				LoadNets(client);
			} catch (Exception ex) {
					WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadSubways", 2000);
			try {
				LoadSubways(client);
			} catch (Exception ex) {
						WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadRegions", 2000);
			try {
							LoadRegions(client);
			} catch (Exception ex) {
							WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadPlaces", 2000);
			try {
				LoadPlaces(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadCategories", 2000);
			try {
				LoadCategories(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadDrugSKUs", 2000);
			try {
				LoadDrugSKUs(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadDrugBrands", 2000);
			try {
				LoadDrugBrands(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadPromotions", 2000);
			try {
				LoadPromotions(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadMessageTypes", 2000);
			try {
				LoadMessageTypes(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadPhotoTypes", 2000);
			try {
				LoadPhotoTypes(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadContracts", 2000);
			try {
				LoadContracts(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadWorkTypes", 2000);
			try {
				LoadWorkTypes(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadMaterials", 2000);
			try {
				LoadMaterials(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadListedHospitals", 2000);
			try {
				LoadListedHospitals(client);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			return true;
		}

		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			Dialog.Window.RequestFeature (WindowFeatures.NoTitle);
			base.OnActivityCreated (savedInstanceState);
			Dialog.Window.Attributes.WindowAnimations = Resource.Style.dialog_animation;
		}

		void Anim_AnimationEnd (object sender, Animation.AnimationEndEventArgs e)
		{
			bSignUp.Visibility = ViewStates.Gone;
			llInfo.Visibility = ViewStates.Visible;
			ThreadPool.QueueUserWorkItem (o => SlowMethod ());
		}

		void WriteInfo(string info, int sleepInMilliseconds = 1)
		{
			activity.RunOnUiThread(() => {
				tvProgressInfo.Text = info;
				Toast.MakeText(context, info, ToastLength.Short).Show();
				llInfo.Visibility = ViewStates.Visible;
			});
			Thread.Sleep (sleepInMilliseconds);
		}

		void WriteSuccess(string info, int sleepInMilliseconds = 1)
		{
			activity.RunOnUiThread(() => {
				tvProgressSuccess.Text = info;
				Toast.MakeText(context, info, ToastLength.Short).Show();
				llInfo.Visibility = ViewStates.Invisible;
				llSuccess.Visibility = ViewStates.Visible;
			});
			Thread.Sleep (sleepInMilliseconds);
		}

		void WriteWarning(string info, int sleepInMilliseconds = 1)
		{
			activity.RunOnUiThread(() => {
				tvProgressWarning.Text = info;
				Toast.MakeText(context, info, ToastLength.Short).Show();
				llInfo.Visibility = ViewStates.Invisible;
				llWarning.Visibility = ViewStates.Visible;
			});
			Thread.Sleep (sleepInMilliseconds);
		}

		void WriteDanger(string info, int sleepInMilliseconds = 1)
		{
			Activity.RunOnUiThread(() => {
				tvProgressDanger.Text = info;
				Toast.MakeText(context, info, ToastLength.Short).Show();
				llInfo.Visibility = ViewStates.Invisible;
				llDanger.Visibility = ViewStates.Visible;
			});

			Thread.Sleep (sleepInMilliseconds);

			activity.RunOnUiThread(() => {
				llWarning.Visibility = ViewStates.Gone;
				llDanger.Visibility = ViewStates.Gone;
				bSignUp.Visibility = ViewStates.Visible;
			});
		}


		void LoadListedHospitals(RestClient client)
		{
			var request = new RestRequest(@"ListedHospital?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<ListedHospital>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format(@"Получено ListedHospital {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadMaterials(RestClient client)
		{
			var request = new RestRequest(@"Material?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<Material>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format(@"Получено Material {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadWorkTypes(RestClient client)
		{
			var request = new RestRequest(@"WorkType?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<WorkType>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format(@"Получено WorkType {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadContracts(RestClient client)
		{
			var request = new RestRequest(@"Contract?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<Contract>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format(@"Получено Contract {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadPhotoTypes(RestClient client)
		{
			var request = new RestRequest(@"PhotoType?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<PhotoType>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format(@"Получено PhotoType {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadMessageTypes(RestClient client)
		{
			var request = new RestRequest(@"MessageType?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<MessageType>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format(@"Получено MessageType {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadPromotions(RestClient client)
		{
			var request = new RestRequest(@"Promotion?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<Promotion>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format(@"Получено Promotion {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadDrugSKUs(RestClient client)
		{
			var request = new RestRequest(@"DrugSKU?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<DrugSKU>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveDrugSKUs(response.Data);
			}
		}

		void LoadDrugBrands(RestClient client)
		{
			var request = new RestRequest(@"DrugBrand?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<DrugBrand>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveDrugBrands(response.Data);
			}
		}

		void LoadPositions(RestClient client)
		{
			var request = new RestRequest(@"Position?limit=300", Method.GET);
			var response = client.Execute<List<Position>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SavePositions(response.Data);
			}
		}

		void LoadNets(RestClient client)
		{
			var request = new RestRequest(@"Net?limit=300", Method.GET);
			var response = client.Execute<List<Net>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveNets(response.Data);
			}
		}

		void LoadSubways(RestClient client)
		{
			var request = new RestRequest(@"Subway?limit=300", Method.GET);
			var response = client.Execute<List<Subway>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveSubways(response.Data);
			}
		}

		void LoadRegions(RestClient client)
		{
			var request = new RestRequest(@"Region?limit=300", Method.GET);
			var response = client.Execute<List<Region>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveRegions(response.Data);
			}
		}

		void LoadPlaces(RestClient client)
		{
			var request = new RestRequest(@"Place?limit=300", Method.GET);
			var response = client.Execute<List<Place>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SavePlaces(response.Data);
			}
		}

		void LoadCategories(RestClient client)
		{
			var request = new RestRequest(@"Category?limit=300", Method.GET);
			var response = client.Execute<List<Category>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveCategories(response.Data);
			}
		}
	}
}
