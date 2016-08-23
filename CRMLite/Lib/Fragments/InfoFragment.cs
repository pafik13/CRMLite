using System;
using System.Collections.Generic;
using System.Linq;

using Android.Support.V4.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using Realms;

using CRMLite.Entities;

namespace CRMLite
{
	public class InfoFragment : Fragment
	{
		public const string ARG_UUID = @"ARG_UUID";

		LayoutInflater Inflater;

		Pharmacy pharmacy;
		IList<Employee> employees;
		IList<DrugBrand> Brands;
		Attendance CurrentAttendance;
		List<PresentationData> presentationDatas;
		List<CoterieData> coterieDatas;
		List<MessageData> messageDatas;
		PromotionData promotionData;
		CompetitorData competitorData;

		//ListView DitributionList;
		//DistributionAdapter Adapter;
		LinearLayout DistributionTable;
		List<Distribution> Distributions;

		//TextSwitcher AttendanceType;
		//string[] AttendanceTypes = { "Презентация", "Фарм-кружок" };
		//int CurrentAttendanceType = 0;
		Transaction Transaction;

		ViewSwitcher AttendanceTypeContent;

		LinearLayout PresentationTable;
		LinearLayout CoterieTable;
		LinearLayout MessageTable;

		public static InfoFragment create(string UUID)
		{
			InfoFragment fragment = new InfoFragment();
			Bundle arguments = new Bundle();
			arguments.PutString(ARG_UUID, UUID);
			fragment.Arguments = arguments;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
			//Transaction = MainDatabase.BeginTransaction();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);

			Inflater = inflater;

			View view = inflater.Inflate(Resource.Layout.InfoFragment, container, false);

			var pharmacyUUID = Arguments.GetString(ARG_UUID);
			if (!string.IsNullOrEmpty(pharmacyUUID))
			{
				pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
				employees = MainDatabase.GetEmployees(pharmacy.UUID);
				Brands = MainDatabase.GetItems<DrugBrand>();
				CurrentAttendance = new Attendance() {
					UUID = Guid.NewGuid().ToString(),
					When = DateTimeOffset.Now
				};
				presentationDatas = new List<PresentationData>();
				coterieDatas = new List<CoterieData>();
				messageDatas = new List<MessageData>();
				promotionData = MainDatabase.Create<PromotionData>(CurrentAttendance.UUID);
				competitorData = MainDatabase.Create<CompetitorData>(CurrentAttendance.UUID);
			}

			//DitributionList = view.FindViewById<ListView>(Resource.Id.ifDistributionTable);

			//View header = inflater.Inflate(Resource.Layout.DistributionTableHeader, DitributionList, false);
			//DitributionList.AddHeaderView(header);

			DistributionTable = view.FindViewById<LinearLayout>(Resource.Id.ifDistributionTable);

			//AttendanceType = view.FindViewById<TextSwitcher>(Resource.Id.ifAttendanceType);
			//AttendanceType.SetInAnimation(Activity, Android.Resource.Animation.SlideInLeft);
			//AttendanceType.SetOutAnimation(Activity, Android.Resource.Animation.SlideOutRight);
			//AttendanceType.SetCurrentText(AttendanceTypes[CurrentAttendanceType]);

			////AttendanceType.Click += (object sender, EventArgs e) =>
			////{
			////	CurrentAttendanceType = (CurrentAttendanceType + 1) % AttendanceTypes.Length;
			////	(sender as TextSwitcher).SetText(AttendanceTypes[CurrentAttendanceType]);
			////};

			//view.FindViewById<Button>(Resource.Id.ifButton).Click += (object sender, EventArgs e) =>
			//{
			//	CurrentAttendanceType = (CurrentAttendanceType + 1) % AttendanceTypes.Length;
			//	AttendanceType.SetText(AttendanceTypes[CurrentAttendanceType]);
			//};

			AttendanceTypeContent = view.FindViewById<ViewSwitcher>(Resource.Id.ifAtendanceTypeContentVS);
			AttendanceTypeContent.SetInAnimation(Activity, Android.Resource.Animation.SlideInLeft);
			AttendanceTypeContent.SetOutAnimation(Activity, Android.Resource.Animation.SlideOutRight);

			view.FindViewById<Button>(Resource.Id.ifChangeAttendanceTypeB).Click += delegate 
			{
				AttendanceTypeContent.ShowNext();
			};

			PresentationTable = view.FindViewById<LinearLayout>(Resource.Id.ifPresentationTable);
			AddPresentationView();
			view.FindViewById<ImageView>(Resource.Id.ifAddPresentationIV).Click += delegate
			{
				AddPresentationView();
			};

			CoterieTable = view.FindViewById<LinearLayout>(Resource.Id.ifCoterieTable);
			AddCoterieView();
			view.FindViewById<ImageView>(Resource.Id.ifAddCoterieIV).Click += delegate
			{
				AddCoterieView();
			};

			var promotionText = view.FindViewById<EditText>(Resource.Id.ifPromotionET);
			var promotions = new List<Promotion>();
			promotions.Add(new Promotion { name = @"Выберите акцию!", uuid = Guid.Empty.ToString() });
			promotions.AddRange(MainDatabase.GetItems<Promotion>());
			var promotion = view.FindViewById<Spinner>(Resource.Id.ifPromotionS);
			var promotionAdapter = new ArrayAdapter(
				Context,
				Android.Resource.Layout.SimpleSpinnerItem,
				promotions.Select(x => x.name).ToArray()
			);
			promotionAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			promotion.Adapter = promotionAdapter;
			promotion.ItemSelected += (sender, e) =>
			{
				if (e.Position == 0) {
					promotionText.Text = string.Empty;
					promotionText.Enabled = false;
				} else {
					promotionText.Enabled = true;
					promotionData.Promotion = promotions[e.Position].uuid;
				}
			};

			promotionText.AfterTextChanged += (sender, e) => {
				promotionData.Text = e.Editable.ToString();
			};

			var competitorText = view.FindViewById<EditText>(Resource.Id.ifCompetitorET);
			var competitor = view.FindViewById<CheckBox>(Resource.Id.ifCompetitorCB);
			competitor.CheckedChange += (sender, e) => {
				if (e.IsChecked) {
					competitorText.Enabled = true;
				}
				else {
					competitorText.Text = string.Empty;
					competitorText.Enabled = false;
				}
			};

			competitorText.AfterTextChanged += (sender, e) => {
				competitorData.Text = e.Editable.ToString();
			};

			MessageTable = view.FindViewById<LinearLayout>(Resource.Id.ifMessageTable);
			AddMessageView();
			view.FindViewById<ImageView>(Resource.Id.ifAddMessageIV).Click += delegate
			{
				AddMessageView();
			};

			return view;
		}

		void AddMessageView()
		{
			var newMessage = new MessageData(); //MainDatabase.CreateMessageData(CurrentAttendance.UUID);

			var message = Inflater.Inflate(Resource.Layout.InfoMessageItem, MessageTable, false);
			message.SetTag(Resource.String.MessageUUID, newMessage.UUID);

			var messageText = message.FindViewById<EditText>(Resource.Id.imiMessageTextET);

			var messageTypes = new List<MessageType>();
			messageTypes.Add(new MessageType { name = @"Выберите тип сообщения!", uuid = Guid.Empty.ToString() });
			messageTypes.AddRange(MainDatabase.GetItems<MessageType>());

			var messageType = message.FindViewById<Spinner>(Resource.Id.imiMessageTypeS);
			var messageTypeAdapter = new ArrayAdapter(
				Context, 
				Android.Resource.Layout.SimpleSpinnerItem, 
				messageTypes.Select(x => x.name).ToArray()
			);
			messageTypeAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			messageType.Adapter = messageTypeAdapter;
			messageType.ItemSelected += (sender, e) =>
			{
				if (e.Position == 0) {
					messageText.Text = string.Empty;
					messageText.Enabled = false;
				}
				else {
					messageText.Enabled = true;
					newMessage.Type = messageTypes[e.Position].uuid;
				}
			};

			messageText.AfterTextChanged += (sender, e) => {
				newMessage.Text = e.Editable.ToString();
			};

			messageDatas.Add(newMessage);
			MessageTable.AddView(message);
		}

		void AddPresentationView()
		{
			var newPresentationData = new PresentationData(); //MainDatabase.CreatePresentationData(CurrentAttendance.UUID);

			var presentation = Inflater.Inflate(Resource.Layout.InfoPresentationItem, PresentationTable, false);
			presentation.SetTag(Resource.String.PresentationDataUUID, newPresentationData.UUID);
			presentation.FindViewById<Button>(Resource.Id.ipiEmployeeForPresentationB).Click += (object sender, EventArgs e) =>
			{
				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите сотрудника аптеки:")
						   .SetCancelable(true)
						   .SetItems(employees.Select(item => item.Name).ToArray(), (caller, arguments) =>
						   {
							   Toast.MakeText(Activity, @"Selected " + arguments.Which, ToastLength.Short).Show();
							   var parent = ((sender as Button).Parent as LinearLayout);
							   var presentationDataUUID = parent.GetTag(Resource.String.PresentationDataUUID).ToString();
							   var presentationData = presentationDatas.Single(item => item.UUID == presentationDataUUID);
							   presentationData.Employee = employees[arguments.Which].UUID;
							   (sender as Button).Text = employees[arguments.Which].Name;
						   })
						   .Show();
			};


			presentation.FindViewById<Button>(Resource.Id.ipiDrugBrandForPresentationB).Click += (object sender, EventArgs e) =>
			{
				var cacheBrands = new List<DrugBrand>(newPresentationData.Brands);

				bool[] checkedItems = new bool[Brands.Count];
				for (int brandIndex = 0; brandIndex < Brands.Count; brandIndex++)
				{
					checkedItems[brandIndex] = cacheBrands.Contains(Brands[brandIndex]);
				}

				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите бренды:")
				           .SetCancelable(false)
				           .SetMultiChoiceItems(
					           Brands.Select(item => item.name).ToArray(),
					           checkedItems,
					           (caller, arguments) => {
								   Toast.MakeText(Activity, @"Selected " + arguments.Which, ToastLength.Short).Show();
								   if (arguments.IsChecked) {
									   cacheBrands.Add(Brands[arguments.Which]);
			   					   } else {
									   cacheBrands.Remove(Brands[arguments.Which]);
								   }
							   }
						   )
				           .SetPositiveButton(
					           @"Сохранить",
							 	(caller, arguments) =>
								{
									newPresentationData.Brands.Clear();
									foreach (var item in cacheBrands) {
										newPresentationData.Brands.Add(item);
									}

									var parent = ((sender as Button).Parent as LinearLayout);
									var text = parent.FindViewById<AutoCompleteTextView>(Resource.Id.ipiDrugBrandForPresentationACTV);
									text.Text = string.Join(",", newPresentationData.Brands.Select(item => item.name));

									(caller as Android.App.Dialog).Dispose();
								}
							)
				           .SetNegativeButton(@"Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						   .Show();
			};


			presentationDatas.Add(newPresentationData);
			PresentationTable.AddView(presentation);
		}

		void AddCoterieView()
		{
			var newCoterieData = new CoterieData(); //MainDatabase.CreateCoterieData(CurrentAttendance.UUID);

			var coterie = Inflater.Inflate(Resource.Layout.InfoCoterieItem, PresentationTable, false);
			coterie.SetTag(Resource.String.CoterieDataUUID, newCoterieData.UUID);

			coterie.FindViewById<Button>(Resource.Id.iciEmployeeForCoterieB).Click += (object sender, EventArgs e) =>
			{
				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите сотрудника аптеки:")
						   .SetCancelable(true)
						   .SetItems(employees.Select(item => item.Name).ToArray(), (caller, arguments) =>
						   {
							   Toast.MakeText(Activity, @"Selected " + arguments.Which, ToastLength.Short).Show();
							   newCoterieData.Employee = employees[arguments.Which].UUID;
							   (sender as Button).Text = employees[arguments.Which].Name;

						   })
						   .Show();
			};


			coterie.FindViewById<Button>(Resource.Id.iciBrandForCoterieB).Click += (object sender, EventArgs e) =>
			{
				var cacheBrands = new List<DrugBrand>(newCoterieData.Brands);

				bool[] checkedItems = new bool[Brands.Count];
				for (int brandIndex = 0; brandIndex < Brands.Count; brandIndex++)
				{
					checkedItems[brandIndex] = cacheBrands.Contains(Brands[brandIndex]);
				}

				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите бренды:")
						   .SetCancelable(false)
						   .SetMultiChoiceItems(
							   Brands.Select(item => item.name).ToArray(),
							   checkedItems,
							   (caller, arguments) =>
							   {
								   Toast.MakeText(Activity, @"Selected " + arguments.Which, ToastLength.Short).Show();
								   if (arguments.IsChecked) {
									   cacheBrands.Add(Brands[arguments.Which]);
								   } else {
									   cacheBrands.Remove(Brands[arguments.Which]);
								   }
							   }
						   )
						    .SetPositiveButton(
							   @"Сохранить",
							 	(caller, arguments) =>
								{
									newCoterieData.Brands.Clear();
									foreach (var item in cacheBrands)
									{
										newCoterieData.Brands.Add(item);
									}

									var parent = ((sender as Button).Parent as LinearLayout);
									var text = parent.FindViewById<AutoCompleteTextView>(Resource.Id.iciBrandForCoterieACTV);
									text.Text = string.Join(",", newCoterieData.Brands.Select(item => item.name));

									(caller as Android.App.Dialog).Dispose();
								}
							)
						    .SetNegativeButton(@"Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						    .Show();
			};

			coterieDatas.Add(newCoterieData);
			CoterieTable.AddView(coterie);
		}

		//public class TextFactory : ViewSwitcher.IViewFactory
		//{
		//	public IntPtr Handle
		//	{
		//		get
		//		{
		//			throw new NotImplementedException();
		//		}
		//	}

		//	public void Dispose()
		//	{
		//		throw new NotImplementedException();
		//	}

		//	public View MakeView()
		//	{
		//		throw new NotImplementedException();
		//	}
		//}

		public override void OnResume()
		{
			base.OnResume();

			//if (Transaction == null) {
			//	Transaction = MainDatabase.BeginTransaction();
			//}

			//realm = Realm.GetInstance();
			//Distributions = new List<Distribution>();
			//List<DrugSKU> drugSKUs = MainDatabase.GetItems<DrugSKU>();

			//foreach (var SKU in drugSKUs)
			//{
			//	Distributions.Add( new Distribution {
			//		UUID = Guid.NewGuid().ToString(),
			//		DrugSKU = SKU.uuid,
			//		IsExistence = false 
			//	});
			//}

			//View header = Activity.LayoutInflater.Inflate(Resource.Layout.DistributionTableHeader, DistributionTable, false);
			//View divider = Activity.LayoutInflater.Inflate(Resource.Layout.Divider, DistributionTable, false);

			//DistributionTable.AddView(header);
			//DistributionTable.AddView(divider);

			//foreach (var item in Distributions)
			//{
			//	var view = (Activity.LayoutInflater.Inflate(
			//						Resource.Layout.DistributionTableItem,
			//						DistributionTable,
			//						false)) as LinearLayout;

			//	view.FindViewById<TextView>(Resource.Id.dtiDrugSKUTV).Text = MainDatabase.GetItem<DrugSKU>(item.DrugSKU).name;
			//	view.FindViewById<CheckBox>(Resource.Id.dtiIsExistenceCB).Checked = item.IsExistence;
			//	view.FindViewById<EditText>(Resource.Id.dtiCountET).Text = item.Count.ToString();
			//	view.FindViewById<EditText>(Resource.Id.dtiPriceET).Text = item.Price.ToString();
			//	view.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked = item.IsPresence;
			//	view.FindViewById<CheckBox>(Resource.Id.dtiIsPOSCB).Checked = item.IsPOS;
			//	view.FindViewById<EditText>(Resource.Id.dtiOrder).Text = item.Order;
			//	view.FindViewById<EditText>(Resource.Id.dtiComment).Text = item.Comment;

			//	DistributionTable.AddView(view);

			//	divider = Activity.LayoutInflater.Inflate(Resource.Layout.Divider, DistributionTable, false);

			//	DistributionTable.AddView(divider);
			//}

			//Adapter = new DistributionAdapter(Activity, Distributions);

			//DitributionList.Adapter = Adapter;
		}

		public override void OnPause()
		{
			base.OnPause();

			//listView.Adapter = null;
			//adapter = null;
			//pharmacies = null;

			//realm.Dispose();
		}

		public override void OnStop()
		{
			base.OnStop();

			//DitributionList.Adapter = null;
			//Adapter = null;
			Distributions = null;
			//MainDatabase.DeletePresentationDatas(presentationDatas);
			//MainDatabase.DeleteCoterieDatas(coterieDatas);
			//Transaction.Commit();
			//Transaction = null;
			//realm.Dispose();
		}
	}
}

