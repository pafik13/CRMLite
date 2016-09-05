using System;
using System.Linq;
using System.Collections.Generic;

using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;

using Realms;

using CRMLite.Entities;

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
		IList<WorkType> WorkTypes;

		DateTimeOffset? AttendanceStart;

		TextView Locker;
		List<MessageData> MessageDatas;
		PromotionData PromotionData;
		CompetitorData CompetitorData;

		LinearLayout DistributionTable;
		//List<Distribution> Distributions;

		LinearLayout SaleTable;
		DateTimeOffset[] SaleDataMonths;
		Dictionary<string, TextView> SaleDataTextViews;

		ViewSwitcher AttendanceTypeContent;
		Button ChangeAttendanceType;
		LinearLayout PresentationTable;
		LinearLayout CoterieLayout;
		LinearLayout PromotionLayout;
		LinearLayout CompetitorLayout;
		LinearLayout MessageLayout;
		LinearLayout MessageTable;
		LinearLayout SaleLayout;
		LinearLayout ResumeLayout;

		View PromotionDivider;
		View CompetitorDivider;
		View MessageDivider;
		View SaleDivider;

		List<MessageType> MessageTypes;
		ImageView MessageAdd;

		TextView CoterieBrands;
		TextView CoterieEmployees;
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
			var attendanceLast = string.IsNullOrEmpty(attendanceLastUUID) ? null : MainDatabase.GetEntity<Attendance>(attendanceLastUUID);

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
			Employees = MainDatabase.GetEmployees(pharmacyUUID);
			Brands = MainDatabase.GetItems<DrugBrand>();
			SKUs = MainDatabase.GetItems<DrugSKU>();
			WorkTypes = MainDatabase.GetItems<WorkType>();

			MessageDatas = new List<MessageData>();
			PromotionData = new PromotionData();
			CompetitorData = new CompetitorData();

			InitDistribution(mainView, attendanceLast);

			InitAttendanceContent(mainView, attendanceLast);

			InitPromotion(mainView, attendanceLast);

			InitCompetitor(mainView, attendanceLast);

			InitMessage(mainView, attendanceLast);

			InitSaleTable(mainView, attendanceLast);

			InitResume(mainView, attendanceLast);

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
			var presentation = Inflater.Inflate(Resource.Layout.InfoPresentationItem, PresentationTable, false);
			var employee = presentation.FindViewById<TextView>(Resource.Id.ipiEmployeeTV);
			employee.Click += (object sender, EventArgs e) => {
				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите сотрудника аптеки:")
						   .SetCancelable(true)
						   .SetItems(Employees.Select(item => item.Name).ToArray(), (caller, arguments) => {
							    employee.SetTag(Resource.String.PDEmployee, Employees[arguments.Which].UUID);
								employee.Text = Employees[arguments.Which].Name;
						   })
						   .Show();
			};

			var brand = presentation.FindViewById<TextView>(Resource.Id.ipiBrandTV);
			brand.Click += (object sender, EventArgs e) => {
				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите бренд препарата:")
						   .SetCancelable(true)
				           .SetItems(Brands.Select(item => item.name).ToArray(), (caller, arguments) => {
								brand.SetTag(Resource.String.PDBrand, Brands[arguments.Which].uuid);
								brand.Text = Brands[arguments.Which].name;
						   })
						   .Show();
			};

			var workTypes = presentation.FindViewById<TextView>(Resource.Id.ipiWorkTypesTV);
			workTypes.Click += WorkTypes_Click;
			PresentationTable.AddView(presentation);
		}

		void WorkTypes_Click(object sender, EventArgs e)
		{
			var tv = (TextView)sender;
			var workTypesUUIDs = (string)tv.GetTag(Resource.String.PDWorkTypes);
			var cacheWorkTypes = string.IsNullOrEmpty(workTypesUUIDs) ? new List<string>() : workTypesUUIDs.Split(';').ToList();

			bool[] checkedItems = new bool[WorkTypes.Count];
			if (cacheWorkTypes.Count > 0) {
				for (int i = 0; i < WorkTypes.Count; i++) {
					checkedItems[i] = cacheWorkTypes.Contains(WorkTypes[i].uuid);
				}
			}

			new Android.App.AlertDialog.Builder(Activity)
					   .SetTitle("Выберите виды работ:")
					   .SetCancelable(false)
					   .SetMultiChoiceItems(
				           WorkTypes.Select(item => item.name).ToArray(),
						   checkedItems,
						   (caller, arguments) => {
							   if (arguments.IsChecked) {
									cacheWorkTypes.Add(WorkTypes[arguments.Which].uuid);
							   } else {
									cacheWorkTypes.Remove(WorkTypes[arguments.Which].uuid);
							   }
						   }
					   )
						.SetPositiveButton(
						   @"Сохранить",
						   (caller, arguments) => {
							   tv.SetTag(Resource.String.PDWorkTypes, string.Join(@";", cacheWorkTypes));
							   if (cacheWorkTypes.Count > 0) {
								   tv.Text = string.Join(System.Environment.NewLine,
														 WorkTypes.Where(wt => cacheWorkTypes.Contains(wt.uuid))
																   .Select(wt => wt.name).ToArray()
														  );
							   } else {
								   tv.Text = @"Выберите виды работ!";
							   }
								(caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton(@"Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
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


			//coterie.FindViewById<Button>(Resource.Id.iciBrandForCoterieB).Click += (object sender, EventArgs e) => {
			//	var cacheBrands = new List<DrugBrand>(newCoterieData.Brands);

			//	bool[] checkedItems = new bool[Brands.Count];
			//	for (int brandIndex = 0; brandIndex < Brands.Count; brandIndex++) {
			//		checkedItems[brandIndex] = cacheBrands.Contains(Brands[brandIndex]);
			//	}

			//	new Android.App.AlertDialog.Builder(Activity)
			//			   .SetTitle("Выберите бренды:")
			//			   .SetCancelable(false)
			//			   .SetMultiChoiceItems(
			//				   Brands.Select(item => item.name).ToArray(),
			//				   checkedItems,
			//				   (caller, arguments) => {
			//					   Toast.MakeText(Activity, @"Selected " + arguments.Which, ToastLength.Short).Show();
			//					   if (arguments.IsChecked) {
			//						   cacheBrands.Add(Brands[arguments.Which]);
			//					   } else {
			//						   cacheBrands.Remove(Brands[arguments.Which]);
			//					   }
			//				   }
			//			   )
			//				.SetPositiveButton(
			//				   @"Сохранить",
			//				 	(caller, arguments) => {
			//						 newCoterieData.Brands.Clear();
			//						 foreach (var item in cacheBrands) {
			//							 newCoterieData.Brands.Add(item);
			//						 }

			//						 var parent = ((sender as Button).Parent as LinearLayout);
			//						 var text = parent.FindViewById<AutoCompleteTextView>(Resource.Id.iciBrandForCoterieACTV);
			//						 text.Text = string.Join(",", newCoterieData.Brands.Select(item => item.name));

			//						 (caller as Android.App.Dialog).Dispose();
			//					 }
			//				)
			//				.SetNegativeButton(@"Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
			//				.Show();
			//};

			//CoterieDatas.Add(newCoterieData);
			//CoterieLayout.AddView(coterie);
		}

		void InitDistribution(View view, Attendance attendanceLast)
		{
			DistributionTable = view.FindViewById<LinearLayout>(Resource.Id.ifDistributionTable);

			View header = Inflater.Inflate(Resource.Layout.DistributionTableHeader, DistributionTable, false);
			View divider = Inflater.Inflate(Resource.Layout.Divider, DistributionTable, false);

			DistributionTable.AddView(header);
			DistributionTable.AddView(divider);

			if (attendanceLast == null) {
				foreach (var SKU in SKUs) {
					var row = (Inflater.Inflate(
										Resource.Layout.DistributionTableItem,
										DistributionTable,
										false)
							  ) as LinearLayout;

					//row.SetTag(Resource.String.DrugSKUUUID, SKU.uuid);
					row.FindViewById<TextView>(Resource.Id.dtiDrugSKUTV).Text = SKU.name;

					DistributionTable.AddView(row);

					divider = Inflater.Inflate(Resource.Layout.Divider, DistributionTable, false);

					DistributionTable.AddView(divider);
				}
			} else {
				var distributions = MainDatabase.GetDistributions(attendanceLast.UUID);
				var dictDistrs = new Dictionary<string, Distribution>();
				foreach (var item in distributions) {
					dictDistrs.Add(item.DrugSKU, item); 
				}

				foreach (var SKU in SKUs) {
					var row = (Inflater.Inflate(
										Resource.Layout.DistributionTableItem,
										DistributionTable,
										false)
							  ) as LinearLayout;
					
					var item = dictDistrs[SKU.uuid];
					row.FindViewById<TextView>(Resource.Id.dtiDrugSKUTV).Text = SKU.name;
					row.FindViewById<CheckBox>(Resource.Id.dtiIsExistenceCB).Checked = item.IsExistence;
					row.FindViewById<EditText>(Resource.Id.dtiCountET).Text = item.Count.ToString();
					row.FindViewById<EditText>(Resource.Id.dtiPriceET).Text = item.Price.ToString();
					row.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked = item.IsPresence;
					row.FindViewById<CheckBox>(Resource.Id.dtiHasPOSCB).Checked = item.HasPOS;
					row.FindViewById<EditText>(Resource.Id.dtiOrderET).Text = item.Order;
					row.FindViewById<EditText>(Resource.Id.dtiCommentET).Text = item.Comment;

					DistributionTable.AddView(row);

					divider = Inflater.Inflate(Resource.Layout.Divider, DistributionTable, false);

					DistributionTable.AddView(divider);
				}
			}
		}

		void InitAttendanceContent(View view, Attendance attendanceLast)
		{
			AttendanceTypeContent = view.FindViewById<ViewSwitcher>(Resource.Id.ifAtendanceTypeContentVS);
			AttendanceTypeContent.SetInAnimation(Activity, Android.Resource.Animation.SlideInLeft);
			AttendanceTypeContent.SetOutAnimation(Activity, Android.Resource.Animation.SlideOutRight);

			ChangeAttendanceType = view.FindViewById<Button>(Resource.Id.ifChangeAttendanceTypeB);
			ChangeAttendanceType.Click += (sender, e) => {
				AttendanceTypeContent.ShowNext();
			};
			// TODO: uncomment
			//ChangeAttendanceType.Visibility = ViewStates.Gone;

			if (attendanceLast == null) {
				AttendanceTypeContent.Visibility = ViewStates.Gone;
				return;
			}

			var presentationsDict = MainDatabase.GetPresentationDatasForView(attendanceLast.UUID);
			if (presentationsDict.Count > 0) {
				View presentationView = Inflater.Inflate(
												Resource.Layout.InfoPresentationData,
												AttendanceTypeContent, 
												false);
				
				var addView = presentationView.FindViewById<ImageView>(Resource.Id.ipdPresentationAddIV);
				addView.Visibility = ViewStates.Gone;

				PresentationTable = presentationView.FindViewById<LinearLayout>(Resource.Id.ipdPresentationTable);

				foreach (var item in presentationsDict) {
					var workTypes = string.Join(System.Environment.NewLine, item.Value.Select(wt => wt.name));

					var presentation = Inflater.Inflate(Resource.Layout.InfoPresentationItem, PresentationTable, false);
					presentation.FindViewById<TextView>(Resource.Id.ipiEmployeeTV).Text = item.Key.Employee.Name;
					presentation.FindViewById<TextView>(Resource.Id.ipiBrandTV).Text = item.Key.Brand.name;
					presentation.FindViewById<TextView>(Resource.Id.ipiWorkTypesTV).Text = workTypes;
					PresentationTable.AddView(presentation);
				}

				AttendanceTypeContent.AddView(presentationView);

				//return;
			}

			var coterieDataGrouped = MainDatabase.GetCoterieDataGrouped(attendanceLast.UUID);

			if (coterieDataGrouped.Brands.Count > 0 || coterieDataGrouped.Employees.Count > 0) {
				CoterieLayout = Inflater.Inflate(
										Resource.Layout.InfoCoterieData,
										AttendanceTypeContent,
										false) as LinearLayout;
				string brands = string.Join(System.Environment.NewLine, coterieDataGrouped.Brands.Values.Select(b => b.name).ToArray());
				CoterieLayout.FindViewById<TextView>(Resource.Id.icdBrandsTV).Text = brands;
				string employees = string.Join(System.Environment.NewLine, coterieDataGrouped.Employees.Values.Select(e => e.Name).ToArray());
				CoterieLayout.FindViewById<TextView>(Resource.Id.icdEmployeesTV).Text = employees;

				AttendanceTypeContent.AddView(CoterieLayout);

				return;
			}

			// если ничего нет, то спрятать
			AttendanceTypeContent.Visibility = ViewStates.Gone;
			//ChangeAttendanceType.Visibility = ViewStates.Gone;
		}

		void InitPromotion(View view, Attendance attendanceLast)
		{
			PromotionLayout = view.FindViewById<LinearLayout>(Resource.Id.ifPromotionLL);
			PromotionDivider = view.FindViewById<View>(Resource.Id.ifPromotionDividerV);

			var promotionText = view.FindViewById<EditText>(Resource.Id.ifPromotionET); 

			var promotionsList = new List<Promotion>();
			promotionsList.Add(new Promotion { name = @"Выберите акцию!", uuid = Guid.Empty.ToString() });
			var promotions = MainDatabase.GetItems<Promotion>();
			promotionsList.AddRange(promotions);
			var promotionSpinner = view.FindViewById<Spinner>(Resource.Id.ifPromotionS);
			var promotionAdapter = new ArrayAdapter(
				Context,
				Android.Resource.Layout.SimpleSpinnerItem,
				promotionsList.Select(x => x.name).ToArray()
			);
			promotionAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			promotionSpinner.Adapter = promotionAdapter;
			//promotionSpinner.ItemSelected += (sender, e) => {
			//	if (e.Position == 0) {
			//		promotionText.Text = string.Empty;
			//		promotionText.Enabled = false;
			//	} else {
			//		promotionText.Enabled = true;
			//		PromotionData.Promotion = promotions[e.Position].uuid;
			//	}
			//};

			if (attendanceLast == null) {
				PromotionLayout.Visibility = ViewStates.Gone;
				PromotionDivider.Visibility = ViewStates.Gone;
				return;
			}

			var promotionData = MainDatabase.GetSingleData<PromotionData>(attendanceLast.UUID);

			if (promotionData == null) {
				PromotionLayout.Visibility = ViewStates.Gone;
				PromotionDivider.Visibility = ViewStates.Gone;
				return;
			}

			promotionText.Text = promotionData.Text;
			for (int p = 1; p < promotionsList.Count; p++) {
				if (promotionsList[p].uuid == promotionData.Promotion) {
					promotionSpinner.SetSelection(p);
					break;
				}	
			}
		}

		void InitCompetitor(View view, Attendance attendanceLast)
		{
			CompetitorLayout = view.FindViewById<LinearLayout>(Resource.Id.ifCompetitorLL);
			CompetitorDivider = view.FindViewById<View>(Resource.Id.ifCompetitorDividerV);

			if (attendanceLast == null) {
				CompetitorLayout.Visibility = ViewStates.Gone;
				CompetitorDivider.Visibility = ViewStates.Gone;
				return;
			}

			var competitorData = MainDatabase.GetSingleData<CompetitorData>(attendanceLast.UUID);

			if (competitorData == null) {
				CompetitorLayout.Visibility = ViewStates.Gone;
				CompetitorDivider.Visibility = ViewStates.Gone;
				return;
			}

			CompetitorLayout.FindViewById<EditText>(Resource.Id.ifCompetitorET).Text = competitorData.Text;
			CompetitorLayout.FindViewById<CheckBox>(Resource.Id.ifCompetitorCB).Checked = true;

			//competitor.CheckedChange += (sender, e) => {
			//	if (e.IsChecked) {
			//		competitorText.Enabled = true;
			//	} else {
			//		competitorText.Text = string.Empty;
			//		competitorText.Enabled = false;
			//	}
			//};

			//competitorText.AfterTextChanged += (sender, e) => {
			//	CompetitorData.Text = e.Editable.ToString();
			//};		
		}

		void InitMessage(View view, Attendance attendanceLast)
		{
			MessageLayout = view.FindViewById<LinearLayout>(Resource.Id.ifMessageLL);
			MessageDivider = view.FindViewById<View>(Resource.Id.ifMessageDividerV);
			MessageTable = view.FindViewById<LinearLayout>(Resource.Id.ifMessageTable);
			MessageAdd = view.FindViewById<ImageView>(Resource.Id.ifMessageAddIV);

			if (attendanceLast == null) {
				MessageLayout.Visibility = ViewStates.Gone;
				MessageDivider.Visibility = ViewStates.Gone;
				return;
			}

			var messages = MainDatabase.GetDatas<MessageData>(attendanceLast.UUID);

			if (messages.Count == 0) {
				MessageLayout.Visibility = ViewStates.Gone;
				MessageDivider.Visibility = ViewStates.Gone;
				return;
			}

			MessageAdd.Visibility = ViewStates.Gone;

			MessageTypes = new List<MessageType>();
			MessageTypes.Add(new MessageType { name = @"Выберите тип сообщения!", uuid = Guid.Empty.ToString() });
			MessageTypes.AddRange(MainDatabase.GetItems<MessageType>());

			foreach (var item in messages) {
				var row = Inflater.Inflate(Resource.Layout.InfoMessageItem, MessageTable, false);
				row.FindViewById<EditText>(Resource.Id.imiMessageTextET).Text = item.Text;
				var type = row.FindViewById<Spinner>(Resource.Id.imiMessageTypeS);
				type.Adapter = new ArrayAdapter(
					Context,
					Android.Resource.Layout.SimpleSpinnerItem,
					MessageTypes.Select(x => x.name).ToArray()
				);

				for (int mt = 1; mt < MessageTypes.Count; mt++) {
					if (MessageTypes[mt].uuid == item.Type) {
						type.SetSelection(mt);
						break;
					}
				}

				MessageTable.AddView(row);
			}
		}

		void InitSaleTable(View view, Attendance attendanceLast)
		{
			SaleLayout = view.FindViewById<LinearLayout>(Resource.Id.ifSaleLL);
			SaleDivider = view.FindViewById<View>(Resource.Id.ifSaleDividerV);
			SaleTable = SaleLayout.FindViewById<LinearLayout>(Resource.Id.ifSaleTable);

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

			var saleDatas = MainDatabase.GetSaleDatas(Pharmacy.UUID, SaleDataMonths);

			if (saleDatas.Count() == 0) {
				SaleLayout.Visibility = ViewStates.Gone;
				SaleDivider.Visibility = ViewStates.Gone;
				return;
			}

			foreach (var sale in saleDatas) {
				key = string.Format("{0}-{1}", sale.DrugSKU, sale.Month.ToString(formatForKey));
				var textView = SaleDataTextViews[key];
				textView.SetTag(Resource.String.SaleDataUUID, sale.UUID);
				textView.Text = sale.Sale == null ? string.Empty : sale.Sale.ToString();
			}
		}

		void InitResume(View view, Attendance attendanceLast)
		{
			ResumeLayout = view.FindViewById<LinearLayout>(Resource.Id.ifResumeLL);
			if (attendanceLast == null) {
				ResumeLayout.Visibility = ViewStates.Gone;
				return;
			}

			var resumeData = MainDatabase.GetSingleData<ResumeData>(attendanceLast.UUID);

			if (resumeData == null) {
				ResumeLayout.Visibility = ViewStates.Gone;
				return;
			}

			ResumeLayout.FindViewById<EditText>(Resource.Id.ifResumeET).Text = resumeData.Text;
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
				Locker.Visibility = ViewStates.Visible;
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

			// 1. Дистрибуция
			for (int c = 2; c < DistributionTable.ChildCount; c = c + 2) {
				var row = DistributionTable.GetChildAt(c) as LinearLayout;
				var isExistence = row.FindViewById<CheckBox>(Resource.Id.dtiIsExistenceCB);
				isExistence.Checked = false;
				isExistence.Enabled = true;
				isExistence.CheckedChange += (sender, e) => {
					var cb = (CheckBox)sender;
					// TODO: danger place!!!
					var ll = (LinearLayout)cb.Parent.Parent;
					ll.FindViewById<EditText>(Resource.Id.dtiCountET).Enabled = e.IsChecked;
					ll.FindViewById<EditText>(Resource.Id.dtiPriceET).Enabled = e.IsChecked;
					ll.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Enabled = e.IsChecked;
					ll.FindViewById<CheckBox>(Resource.Id.dtiHasPOSCB).Enabled = e.IsChecked;
					ll.FindViewById<EditText>(Resource.Id.dtiOrderET).Enabled = e.IsChecked;
					ll.FindViewById<EditText>(Resource.Id.dtiCommentET).Enabled = e.IsChecked;
				};
				row.FindViewById<EditText>(Resource.Id.dtiCountET).Text = string.Empty;
				row.FindViewById<EditText>(Resource.Id.dtiPriceET).Text = string.Empty;
				row.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked = false;
				row.FindViewById<CheckBox>(Resource.Id.dtiHasPOSCB).Checked = false;
				row.FindViewById<EditText>(Resource.Id.dtiOrderET).Text = string.Empty;
				row.FindViewById<EditText>(Resource.Id.dtiCommentET).Text = string.Empty;
			}

			// 2. Содержание визита
			AttendanceTypeContent.RemoveAllViews();

			// 2.1 Презентация
			View presentationView = Inflater.Inflate(
											Resource.Layout.InfoPresentationData,
											AttendanceTypeContent,
											false);
			presentationView.FindViewById<ImageView>(Resource.Id.ipdPresentationAddIV).Click += (s, e) => {
				AddPresentationView();
			};

			PresentationTable = presentationView.FindViewById<LinearLayout>(Resource.Id.ipdPresentationTable);
			AddPresentationView();
			AttendanceTypeContent.AddView(presentationView);

			// 2.1 Фарм-кружок
			CoterieLayout = Inflater.Inflate(
									Resource.Layout.InfoCoterieData,
									AttendanceTypeContent,
									false) as LinearLayout;
			CoterieBrands = CoterieLayout.FindViewById<TextView>(Resource.Id.icdBrandsTV);
			CoterieBrands.Click += CoterieBrands_Click;
			CoterieEmployees = CoterieLayout.FindViewById<TextView>(Resource.Id.icdEmployeesTV);
			CoterieEmployees.Click += CoterieEmployees_Click;
			AttendanceTypeContent.AddView(CoterieLayout);
		}

		void CoterieEmployees_Click(object sender, EventArgs e)
		{
			var tv = (TextView)sender;

			var employeesUUIDs = (string)tv.GetTag(Resource.String.CDEmployees);
			var cacheEmployees = string.IsNullOrEmpty(employeesUUIDs) ? new List<string>() : employeesUUIDs.Split(';').ToList();

			bool[] checkedItems = new bool[Employees.Count];
			if (cacheEmployees.Count > 0) {
				for (int i = 0; i < Employees.Count; i++) {
					checkedItems[i] = cacheEmployees.Contains(Employees[i].UUID);
				}
			}

			new Android.App.AlertDialog.Builder(Activity)
					   .SetTitle("Выберите сотрудников:")
					   .SetCancelable(false)
					   .SetMultiChoiceItems(
				           Employees.Select(item => item.Name).ToArray(),
						   checkedItems,
						   (caller, arguments) => {
							   if (arguments.IsChecked) {
								   cacheEmployees.Add(Employees[arguments.Which].UUID);
							   } else {
								   cacheEmployees.Remove(Employees[arguments.Which].UUID);
							   }
						   }
					   )
						.SetPositiveButton(
						   @"Сохранить",
						   (caller, arguments) => {
								tv.SetTag(Resource.String.CDEmployees, string.Join(@";", cacheEmployees));
							   if (cacheEmployees.Count > 0) {
								   tv.Text = string.Join(System.Environment.NewLine,
						                                 Employees.Where(emp => cacheEmployees.Contains(emp.UUID))
						                                 		  .Select(emp => emp.Name).ToArray()
														  );
							   } else {
								   tv.Text = @"Выберите сотрудников!";
							   }
								(caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton(@"Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
		}

		void CoterieBrands_Click(object sender, EventArgs e)
		{
			var tv = (TextView)sender;

			var brandsUUIDs = (string)tv.GetTag(Resource.String.CDBrands);
			var cacheBrands = string.IsNullOrEmpty(brandsUUIDs) ? new List<string>() : brandsUUIDs.Split(';').ToList();

			bool[] checkedItems = new bool[Brands.Count];
			if (cacheBrands.Count > 0) {
				for (int brandIndex = 0; brandIndex < Brands.Count; brandIndex++) {
					checkedItems[brandIndex] = cacheBrands.Contains(Brands[brandIndex].uuid);
				}
			}

			new Android.App.AlertDialog.Builder(Activity)
					   .SetTitle("Выберите бренды:")
					   .SetCancelable(false)
					   .SetMultiChoiceItems(
						   Brands.Select(item => item.name).ToArray(),
						   checkedItems,
						   (caller, arguments) => {
							   if (arguments.IsChecked) {
								   cacheBrands.Add(Brands[arguments.Which].uuid);
							   } else {
								   cacheBrands.Remove(Brands[arguments.Which].uuid);
							   }
						   }
					   )
						.SetPositiveButton(
						   @"Сохранить",
						   (caller, arguments) => {
							   tv.SetTag(Resource.String.CDBrands, string.Join(@";", cacheBrands));
							   if (cacheBrands.Count > 0) {
								   tv.Text = string.Join(System.Environment.NewLine, 
						                                 Brands.Where(b => cacheBrands.Contains(b.uuid))
						                                 	   .Select(b => b.name).ToArray()
						                      			);
							   } else {
								   tv.Text = @"Выберите бренды!";
							   }
								(caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton(@"Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
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
				distr.Comment = row.FindViewById<EditText>(Resource.Id.dtiCommentET).Text;
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

