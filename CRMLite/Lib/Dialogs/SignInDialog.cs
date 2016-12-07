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
	public class HostHolder
	{
		public string hostURL { get; set;}
	}

	public class SigninDialog : DialogFragment
	{
		const char quote = '"';
		public const string C_USERNAME = @"C_USERNAME";
		public const string C_EMAIL = @"C_EMAIL";
		public const string C_ACCESS_TOKEN = @"C_ACCESS_TOKEN";
		public const string C_AGENT_UUID = @"C_AGENT_UUID";
		public const string C_HOST_URL = @"C_HOST_URL";


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

		//string cookieName = "";
		//string cookieValue = "";

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

			Dialog.SetCanceledOnTouchOutside (false);
			Cancelable = false;
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
				Cancelable = true;
				WriteSuccess(@"ВХОД ВЫПОЛНЕН УСПЕШНО", 3000);
				MainDatabase.Dispose();
				activity.RunOnUiThread(() => MainDatabase.Username = tvUsername.Text);
				OnSuccessSignedIn(EventArgs.Empty);
				Dismiss();
				//				MessageBox.Show()
			}
			else
			{
				activity.RunOnUiThread(() => Toast.MakeText(context, @"NOT Authentificated", ToastLength.Short).Show());
				WriteDanger (@"ОШИБКА! ВХОД НЕ ВЫПОЛНЕН", 3000);
				MainDatabase.Dispose();

				//activity.GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private)
				//		.Edit()
				//        .PutString(C_USERNAME, string.Empty)
				//		.PutString(C_ACCESS_TOKEN, (response as IRestResponse<JsonWebToken>).Data.token)
				//		.Commit();
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
			WriteInfo ("Подключение к серверу");

			var login = new RestClient(@"http://front-sblcrm.rhcloud.com/");
			//var client = new RestClient(@"http://sbl-crm-project-pafik13.c9users.io:8080/");
			login.CookieContainer = new CookieContainer();

			string access_token = string.Empty;
			string email = username + "@sbl-crm.ru";
			IRestResponse response;
			WriteInfo("Получение Access_Token", 1000);
			try {
				var request = new RestRequest(@"auth/login", Method.POST);
				request.AddParameter("email", email, ParameterType.GetOrPost);
				request.AddParameter("password", password, ParameterType.GetOrPost);
				response = login.Execute(request);
				if (response.StatusCode != HttpStatusCode.OK) {
					return false;
				}

				request = new RestRequest(@"user/jwt", Method.GET);
				response = login.Execute<JsonWebToken>(request);
				if (response.StatusCode != HttpStatusCode.OK) {
					return false;
				}

				access_token = (response as IRestResponse<JsonWebToken>).Data.token;
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			MainDatabase.Username = username;

			Helper.Username = username;

			Agent agent;
			string hostURL;
			WriteInfo(@"Получение Agent", 1000);
			try {
				string path = typeof(Agent).Name + @"/byjwt";

				var request = new RestRequest(path, Method.GET);
				request.AddQueryParameter(@"access_token", access_token);
				response = login.Execute<Agent>(request);
				agent = (response as IRestResponse<Agent>).Data;

				using (var trans = MainDatabase.BeginTransaction()) {
					MainDatabase.DeleteAll<Agent>(trans);
					MainDatabase.SaveItem(trans, agent);
					trans.Commit();
				}

				request = new RestRequest(path, Method.GET);
				request.AddQueryParameter(@"access_token", access_token);
				response = login.Execute<HostHolder>(request);
				hostURL = (response as IRestResponse<HostHolder>).Data.hostURL;
				Uri uriResult;
				bool result = Uri.TryCreate(hostURL, UriKind.Absolute, out uriResult)
					&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
				if (!result) return false;
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			string agentCityWhere = @"where={""city"": """ + agent.city + @"""}";

			activity.GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private)
					.Edit()
					.PutString(C_USERNAME, username)
					.PutString(C_EMAIL, email)
					.PutString(C_ACCESS_TOKEN, access_token)
					.PutString(C_AGENT_UUID, agent.uuid)
			        .PutString(C_HOST_URL, hostURL)
					.Commit();

			var client = new RestClient(hostURL);

			WriteInfo(@"Получение аптек", 1000);
			try {
				LoadEntities<Pharmacy>(client, access_token);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение сотрудников", 1000);
			try {
				LoadEntities<Employee>(client, access_token);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadPositions", 1000);
			try {
				//LoadPositions(client);
				LoadItems<Position>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadNets", 1000);
			try {
				//LoadNets(client);
				LoadItems<Net>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadSubways", 1000);
			try {
				//LoadSubways(client);
				LoadItems<Subway>(client, 300, agentCityWhere);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadRegions", 1000);
			try {
				//LoadRegions(client);
				LoadItems<Region>(client, 300, agentCityWhere);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadPlaces", 1000);
			try {
				//LoadPlaces(client);
				LoadItems<Place>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadCategories", 1000);
			try {
				//LoadCategories(client);
				LoadItems<Category>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadDrugSKUs", 1000);
			try {
				//LoadDrugSKUs(client);
				LoadItems<DrugSKU>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadDrugBrands", 1000);
			try {
				//LoadDrugBrands(client);
				LoadItems<DrugBrand>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadPromotions", 1000);
			try {
				//LoadPromotions(client);
				LoadItems<Promotion>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadMessageTypes", 1000);
			try {
				//LoadMessageTypes(client);
				LoadItems<MessageType>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadPhotoTypes", 1000);
			try {
				//LoadPhotoTypes(client);
				LoadItems<PhotoType>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadContracts", 1000);
			try {
				//LoadContracts(client);
				LoadItems<Contract>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadWorkTypes", 1000);
			try {
				//LoadWorkTypes(client);
				LoadItems<WorkType>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadMaterials", 1000);
			try {
				//LoadMaterials(client);
				LoadItems<Material>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение LoadListedHospitals", 1000);
			try {
				//LoadListedHospitals(client);
				LoadItems<ListedHospital>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение Distributors", 1000);
			try {
				LoadItems<Distributor>(client, 300);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение DistributionAgreements", 1000);
			try {
				LoadItems<DistributionAgreement>(client, 500);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение PhotoAgreements", 1000);
			try {
				LoadItems<PhotoAgreement>(client, 500);
			} catch (Exception ex) {
				WriteWarning(string.Format(@"Error: {0}", ex.Message), 2000);
				return false;
			}

			WriteInfo(@"Получение DistributorRemains", 1000);
			try {
				LoadItems<DistributorRemain>(client, 500);
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
				//Toast.MakeText(context, info, ToastLength.Short).Show();
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
				using (var trans = MainDatabase.BeginTransaction()) {
					MainDatabase.DeleteAll<Position>(trans);
					MainDatabase.SaveItems(trans, response.Data);
					trans.Commit();
				}
			}
		}

		void LoadItems<T>(RestClient client, int limit, string customQueryParams = "") where T: Realms.RealmObject
		{
			Console.WriteLine(@"LoadItems: typeof={0}", typeof(T));
			string className = typeof(T).Name;
			string path = string.Format(@"{0}?limit={1}&populate=false&{2}", className, limit, customQueryParams);
			var request = new RestRequest(path, Method.GET);
			var response = client.Execute<List<T>>(request);
			if (response.StatusCode == HttpStatusCode.OK) {
				Console.WriteLine(@"LoadItems: Data.Count={0}", response.Data.Count);
				using (var trans = MainDatabase.BeginTransaction()) {
					MainDatabase.DeleteAll<T>(trans);
					MainDatabase.SaveItems(trans, response.Data);
					trans.Commit();
				}
			}
			Console.WriteLine(@"LoadItems: Done");
		}

		void LoadEntities<T>(RestClient client, string access_token) where T : Realms.RealmObject, IEntity, ISync
		{
			Console.WriteLine(@"LoadEntities: typeof={0}", typeof(T));
			string className = typeof(T).Name;
			string path = string.Format(@"{0}/byjwt?access_token={1}", className, access_token);
			var request = new RestRequest(path, Method.GET);
			var response = client.Execute<List<T>>(request);
			if (response.StatusCode == HttpStatusCode.OK) {
				Console.WriteLine(@"LoadEntities: Data.Count={0}", response.Data.Count);
				using (var trans = MainDatabase.BeginTransaction()) {
					MainDatabase.DeleteAll<T>(trans);
					MainDatabase.SaveEntities(trans, response.Data);
					trans.Commit();
				}
			}
			Console.WriteLine(@"LoadEntities: Done");
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
