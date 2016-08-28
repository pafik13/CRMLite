using System;
using System.Collections.Generic;
using System.Linq;

using Android.Support.V4.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using CRMLite.Dialogs;
using CRMLite.Entities;
using Realms;

namespace CRMLite
{
	public class InfoFragment : Fragment, IAttendanceControl
	{
		public const string C_PHARMACY_UUID = @"C_PHARMACY_UUID";
		public const string C_ATTENDANCE_LAST_UUID = @"C_ATTENDANCE_LAST_UUID";

		LayoutInflater Inflater;

		Pharmacy Pharmacy;
		IList<Employee> Employees;
		IList<DrugBrand> Brands;
		IList<DrugSKU> SKUs;
		Attendance AttendanceLast;

		DateTimeOffset? AttendanceStart;

		TextView Locker;
		List<PresentationData> PresentationDatas;
		List<CoterieData> CoterieDatas;
		List<MessageData> MessageDatas;
		PromotionData PromotionData;
		CompetitorData CompetitorData;

		LinearLayout DistributionTable;
		//List<Distribution> Distributions;

		LinearLayout SaleTable;
		DateTimeOffset[] SaleDataMonths;
		Dictionary<string, TextView> SaleDataTextViews;

		ViewSwitcher AttendanceTypeContent;

		LinearLayout PresentationTable;
		LinearLayout CoterieTable;
		LinearLayout MessageTable;

		public static InfoFragment create(string pharmacyUUID, string attendanceLastUUID)
		{
			InfoFragment fragment = new InfoFragment();
			Bundle arguments = new Bundle();
			arguments.PutString(C_PHARMACY_UUID, pharmacyUUID);
			arguments.PutString(C_ATTENDANCE_LAST_UUID, attendanceLastUUID);
			fragment.Arguments = arguments;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);

			Inflater = inflater;

			View mainView = inflater.Inflate(Resource.Layout.InfoFragment, container, false);

			var pharmacyUUID = Arguments.GetString(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return mainView;

			var attendanceLastUUID = Arguments.GetString(C_ATTENDANCE_LAST_UUID);
			AttendanceLast = string.IsNullOrEmpty(attendanceLastUUID) ? null : MainDatabase.GetEntity<Attendance>(attendanceLastUUID);

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
			Employees = MainDatabase.GetEmployees(Pharmacy.UUID);
			Brands = MainDatabase.GetItems<DrugBrand>();
			SKUs = MainDatabase.GetItems<DrugSKU>();

			PresentationDatas = new List<PresentationData>();
			CoterieDatas = new List<CoterieData>();
			MessageDatas = new List<MessageData>();
			PromotionData = new PromotionData();
			CompetitorData = new CompetitorData();

			InitDistributionTable(mainView);

			InitAttendanceContent(mainView);

			InitPromotion(mainView);

			InitCompetitor(mainView);

			InitMessage(mainView);

			InitSaleTable(mainView);

			Locker = mainView.FindViewById<TextView>(Resource.Id.locker);

			return mainView;
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
			messageType.ItemSelected += (sender, e) => {
				if (e.Position == 0) {
					messageText.Text = string.Empty;
					messageText.Enabled = false;
				} else {
					messageText.Enabled = true;
					newMessage.Type = messageTypes[e.Position].uuid;
				}
			};

			messageText.AfterTextChanged += (sender, e) => {
				newMessage.Text = e.Editable.ToString();
			};

			MessageDatas.Add(newMessage);
			MessageTable.AddView(message);
		}

		void AddPresentationView()
		{
			var newPresentationData = new PresentationData(); //MainDatabase.CreatePresentationData(CurrentAttendance.UUID);

			var presentation = Inflater.Inflate(Resource.Layout.InfoPresentationItem, PresentationTable, false);
			presentation.SetTag(Resource.String.PresentationDataUUID, newPresentationData.UUID);
			presentation.FindViewById<Button>(Resource.Id.ipiEmployeeForPresentationB).Click += (object sender, EventArgs e) => {
				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите сотрудника аптеки:")
						   .SetCancelable(true)
						   .SetItems(Employees.Select(item => item.Name).ToArray(), (caller, arguments) => {
							   Toast.MakeText(Activity, @"Selected " + arguments.Which, ToastLength.Short).Show();
							   var parent = ((sender as Button).Parent as LinearLayout);
							   var presentationDataUUID = parent.GetTag(Resource.String.PresentationDataUUID).ToString();
							   var presentationData = PresentationDatas.Single(item => item.UUID == presentationDataUUID);
							   presentationData.Employee = Employees[arguments.Which].UUID;
							   (sender as Button).Text = Employees[arguments.Which].Name;
						   })
						   .Show();
			};


			presentation.FindViewById<Button>(Resource.Id.ipiDrugBrandForPresentationB).Click += (object sender, EventArgs e) => {
				var cacheBrands = new List<DrugBrand>(newPresentationData.Brands);

				bool[] checkedItems = new bool[Brands.Count];
				for (int brandIndex = 0; brandIndex < Brands.Count; brandIndex++) {
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
							 	(caller, arguments) => {
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


			PresentationDatas.Add(newPresentationData);
			PresentationTable.AddView(presentation);
		}

		void AddCoterieView()
		{
			var newCoterieData = new CoterieData(); //MainDatabase.CreateCoterieData(CurrentAttendance.UUID);

			var coterie = Inflater.Inflate(Resource.Layout.InfoCoterieItem, PresentationTable, false);
			coterie.SetTag(Resource.String.CoterieDataUUID, newCoterieData.UUID);

			coterie.FindViewById<Button>(Resource.Id.iciEmployeeForCoterieB).Click += (object sender, EventArgs e) => {
				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите сотрудника аптеки:")
						   .SetCancelable(true)
						   .SetItems(Employees.Select(item => item.Name).ToArray(), (caller, arguments) => {
							   Toast.MakeText(Activity, @"Selected " + arguments.Which, ToastLength.Short).Show();
							   newCoterieData.Employee = Employees[arguments.Which].UUID;
							   (sender as Button).Text = Employees[arguments.Which].Name;

						   })
						   .Show();
			};


			coterie.FindViewById<Button>(Resource.Id.iciBrandForCoterieB).Click += (object sender, EventArgs e) => {
				var cacheBrands = new List<DrugBrand>(newCoterieData.Brands);

				bool[] checkedItems = new bool[Brands.Count];
				for (int brandIndex = 0; brandIndex < Brands.Count; brandIndex++) {
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
							 	(caller, arguments) => {
									 newCoterieData.Brands.Clear();
									 foreach (var item in cacheBrands) {
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

			CoterieDatas.Add(newCoterieData);
			CoterieTable.AddView(coterie);
		}

		void InitDistributionTable(View view)
		{
			DistributionTable = view.FindViewById<LinearLayout>(Resource.Id.ifDistributionTable);

			View header = Inflater.Inflate(Resource.Layout.DistributionTableHeader, DistributionTable, false);
			View divider = Inflater.Inflate(Resource.Layout.Divider, DistributionTable, false);

			DistributionTable.AddView(header);
			DistributionTable.AddView(divider);

			foreach (var SKU in SKUs) {
				var row = (Inflater.Inflate(
									Resource.Layout.DistributionTableItem,
									DistributionTable,
									false)) as LinearLayout;
				
				row.SetTag(Resource.String.DrugSKUUUID, SKU.uuid);
				row.FindViewById<TextView>(Resource.Id.dtiDrugSKUTV).Text = SKU.name;

				DistributionTable.AddView(row);

				divider = Activity.LayoutInflater.Inflate(Resource.Layout.Divider, DistributionTable, false);

				DistributionTable.AddView(divider);
			}
		}

		void InitAttendanceContent(View view)
		{
			AttendanceTypeContent = view.FindViewById<ViewSwitcher>(Resource.Id.ifAtendanceTypeContentVS);
			AttendanceTypeContent.SetInAnimation(Activity, Android.Resource.Animation.SlideInLeft);
			AttendanceTypeContent.SetOutAnimation(Activity, Android.Resource.Animation.SlideOutRight);

			view.FindViewById<Button>(Resource.Id.ifChangeAttendanceTypeB).Click += (sender, e) => {
				AttendanceTypeContent.ShowNext();
			};

			PresentationTable = view.FindViewById<LinearLayout>(Resource.Id.ifPresentationTable);
			AddPresentationView();
			view.FindViewById<ImageView>(Resource.Id.ifAddPresentationIV).Click += (sender, e) => {
				AddPresentationView();
			};

			CoterieTable = view.FindViewById<LinearLayout>(Resource.Id.ifCoterieTable);
			AddCoterieView();
			view.FindViewById<ImageView>(Resource.Id.ifAddCoterieIV).Click += (sender, e) => {
				AddCoterieView();
			};
		}

		void InitPromotion(View view)
		{
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
			promotion.ItemSelected += (sender, e) => {
				if (e.Position == 0) {
					promotionText.Text = string.Empty;
					promotionText.Enabled = false;
				} else {
					promotionText.Enabled = true;
					PromotionData.Promotion = promotions[e.Position].uuid;
				}
			};

			promotionText.AfterTextChanged += (sender, e) => {
				PromotionData.Text = e.Editable.ToString();
			};
		}

		void InitCompetitor(View view)
		{
			var competitorText = view.FindViewById<EditText>(Resource.Id.ifCompetitorET);
			var competitor = view.FindViewById<CheckBox>(Resource.Id.ifCompetitorCB);
			competitor.CheckedChange += (sender, e) => {
				if (e.IsChecked) {
					competitorText.Enabled = true;
				} else {
					competitorText.Text = string.Empty;
					competitorText.Enabled = false;
				}
			};

			competitorText.AfterTextChanged += (sender, e) => {
				CompetitorData.Text = e.Editable.ToString();
			};		
		}

		void InitMessage(View mainView)
		{
			MessageTable = mainView.FindViewById<LinearLayout>(Resource.Id.ifMessageTable);
			AddMessageView();
			mainView.FindViewById<ImageView>(Resource.Id.ifAddMessageIV).Click += (sender, e) => {
				AddMessageView();
			};
		}

		void InitSaleTable(View view)
		{
			SaleTable = view.FindViewById<LinearLayout>(Resource.Id.ifSaleTable);

			var header = (LinearLayout)Inflater.Inflate(Resource.Layout.SaleTableHeader, SaleTable, false);
			SaleDataMonths = new DateTimeOffset[8];
			foreach (var SKU in SKUs) {
				for (int m = 0; m < 8; m++) {
					var month = DateTimeOffset.Now.AddMonths(-6 + m);
					SaleDataMonths[m] = month;
					var hView = header.GetChildAt(m + 1);
					if (hView is TextView) {
						(hView as TextView).Text = month.ToString(string.Format(@"MMMM{0}yyyy", System.Environment.NewLine));
					}
				}
			}

			View divider = Inflater.Inflate(Resource.Layout.Divider, SaleTable, false);

			SaleTable.AddView(header);
			SaleTable.AddView(divider);

			var key = string.Empty;
			var formatForKey = @"MMyy";
			SaleDataTextViews = new Dictionary<string, TextView>();
			foreach (var SKU in SKUs) {
				var row = (Inflater.Inflate(
									Resource.Layout.SaleTableItem,
									SaleTable,
									false)) as LinearLayout;
				row.SetTag(Resource.String.DrugSKUUUID, SKU.uuid);
				row.FindViewById<TextView>(Resource.Id.stiDrugSKUTV).Text = SKU.name;

				for (int c = 1; c < row.ChildCount; c++) {
					var rView = (TextView)row.GetChildAt(c);
					rView.SetTag(Resource.String.IsChanged, false);
					rView.AfterTextChanged += RView_AfterTextChanged;

					key = string.Format("{0}-{1}", SKU.uuid, SaleDataMonths[c - 1].ToString(formatForKey));
					SaleDataTextViews.Add(key, rView);
 				}
				
				SaleTable.AddView(row);

				divider = Inflater.Inflate(Resource.Layout.Divider, SaleTable, false);

				SaleTable.AddView(divider);
			}

			var formatForMonthCompare = @"yyyyMM";
			var months = SaleDataMonths.Select(m => m.ToString(formatForMonthCompare));
			var saleDatas = MainDatabase.GetItems<SaleData>()
										.Where(s => MainDatabase.GetEntity<Attendance>(s.Attendance).Pharmacy == Pharmacy.UUID)
			                            .Where(s => months.Contains(s.Month.ToString(formatForMonthCompare)));

			foreach (var sale in saleDatas) {
				key = string.Format("{0}-{1}", sale.DrugSKU, sale.Month.ToString(formatForKey));
				var textView = SaleDataTextViews[key];
				textView.SetTag(Resource.String.SaleDataUUID, sale.UUID);
				textView.Text = sale.Sale == null ? string.Empty : sale.Sale.ToString();

			}
		}

		void RView_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
		{
			var textView = sender as TextView;
			textView.SetTag(Resource.String.IsChanged, true);
		}

		public override void OnResume()
		{
			base.OnResume();
			if (Pharmacy == null) {
				new Android.App.AlertDialog.Builder(Context)
								   .SetTitle(Resource.String.error_caption)
								   .SetMessage("Отсутствует аптека!")
								   .SetCancelable(false)
								   .SetPositiveButton(@"OK", (dialog, args) => {
									   if (dialog is Android.App.Dialog) {
										   ((Android.App.Dialog)dialog).Dismiss();
									   }
								   })
								   .Show();

			} else if (AttendanceStart == null) {
				Locker.Visibility = ViewStates.Visible;;
			}
		}

		public override void OnPause()
		{
			base.OnPause();
		}

		public override void OnStop()
		{
			base.OnStop();
		}

		public void OnAttendanceStart(DateTimeOffset? start)
		{
			AttendanceStart = start;
			Locker.Visibility = ViewStates.Gone;
		}

		public void OnAttendanceStop(Transaction openedTransaction, Attendance current)
		{
			if (openedTransaction == null) {
				throw new ArgumentNullException(nameof(openedTransaction));
			}

			// Save Distributions
			for (int c = 2; c < DistributionTable.ChildCount; c = c + 2) {
				var row = (LinearLayout)DistributionTable.GetChildAt(c);
				var distr = MainDatabase.CreateData<Distribution>(current.UUID);
				distr.DrugSKU = (string)row.GetTag(Resource.String.DrugSKUUUID);
				distr.IsExistence = row.FindViewById<CheckBox>(Resource.Id.dtiIsExistenceCB).Checked;
				distr.Count = Helper.ToFloatExeptNull(row.FindViewById<EditText>(Resource.Id.dtiCountET).Text);
				distr.Price = Helper.ToFloatExeptNull(row.FindViewById<EditText>(Resource.Id.dtiPriceET).Text);
				distr.IsPresence = row.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked;
				distr.HasPOS = row.FindViewById<CheckBox>(Resource.Id.dtiHasPOSCB).Checked;
				distr.Order = row.FindViewById<EditText>(Resource.Id.dtiOrderET).Text;
				distr.Comment = row.FindViewById<EditText>(Resource.Id.dtiCommentET).Text;;
			}

			// Save Sale
			for (int c = 2; c < SaleTable.ChildCount; c = c + 2) {
				var row = (LinearLayout)SaleTable.GetChildAt(c);
				var skuUUID = (string)row.GetTag(Resource.String.DrugSKUUUID);
				for (int m = 0; m < 8; m++) {
					var rView = (TextView)row.GetChildAt(m + 1);
					if (string.IsNullOrEmpty(rView.Text)) continue;

					var isChanged = (bool)rView.GetTag(Resource.String.IsChanged);
					if (isChanged) {
						var saleUUID = (string)rView.GetTag(Resource.String.SaleDataUUID);

						Console.WriteLine("Info: sku = {0}, value = {1}, sale = {2}", skuUUID, Helper.ToFloat(rView.Text), saleUUID);

						if (string.IsNullOrEmpty(saleUUID)) {
							var sale = MainDatabase.CreateData<SaleData>(current.UUID);
							sale.DrugSKU = skuUUID;
							sale.Month = SaleDataMonths[m];
							sale.Sale = Helper.ToFloat(rView.Text);
						} else {
							var sale = MainDatabase.GetEntity<SaleData>(saleUUID);
							Console.WriteLine("Info: sku = {0}, month = {1}", sale.DrugSKU == skuUUID, sale.Month.Month == SaleDataMonths[m].Month);
							sale.Sale = Helper.ToFloat(rView.Text);
						}
					}
				}
			}
		}
	}
}

