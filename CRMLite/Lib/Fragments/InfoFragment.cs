﻿using System;
using System.Linq;
using System.Collections.Generic;

using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;

using Realms;

using CRMLite.Entities;
using Android.Text;
using InputFilterForEditText;
using System.Diagnostics;

namespace CRMLite
{
	public class InfoFragment : Fragment, IAttendanceControl
	{
		Stopwatch Chrono;

		public const string C_PHARMACY_UUID = @"C_PHARMACY_UUID";
		public const string C_ATTENDANCE_LAST_UUID = @"C_ATTENDANCE_LAST_UUID";

		string HOST_URL;

		Pharmacy Pharmacy;

		IList<Employee> Employees;
		IList<DrugBrand> Brands;
		IList<DrugSKU> SKUs;
		Dictionary<string, DrugSKU> SKUsDict;

		IList<WorkType> WorkTypes;
		IList<Promotion> Promotions;
		List<Promotion> PromotionsList;
		IList<MessageType> MessageTypes;

		DateTimeOffset? AttendanceStart;

		TextView Locker;

		ImageView Arrow;

		LinearLayout SaleTable;
		DateTimeOffset[] SaleDataMonths;
		Dictionary<string, TextView> SaleDataTextViews;

		// Link on Views
		Dictionary<string, DistributionAgreement> Agreements;
		Dictionary<string, DistributionData> DistributionDatasDict;
		TableLayout DistributionTable;


		ViewSwitcher AttendanceTypeContent;
		//Button ChangeAttendanceType;
		LinearLayout PresentationTable;

		RelativeLayout CoterieLayout;

		PromotionData PromotionData;
		LinearLayout PromotionLayout;

		CompetitorData CompetitorData;
		LinearLayout CompetitorLayout;
		//LinearLayout MessageLayout;
		//LinearLayout MessageTable;
		//LinearLayout SaleLayout;
		ResumeData ResumeData;
		LinearLayout ResumeLayout;

		//View PromotionDivider;
		//View CompetitorDivider;
		//View MessageDivider;
		//View SaleDivider;

		//TextView CoterieBrands;
		//TextView CoterieEmployees;



		public static InfoFragment create(string pharmacyUUID, string attendanceLastUUID, bool isAttendanceRunning)
		{
			InfoFragment fragment = new InfoFragment();
			Bundle arguments = new Bundle();
			arguments.PutString(C_PHARMACY_UUID, pharmacyUUID);
			arguments.PutString(C_ATTENDANCE_LAST_UUID, attendanceLastUUID);
			arguments.PutBoolean(Consts.C_IS_ATTENDANCE_RUNNING, isAttendanceRunning);
			fragment.Arguments = arguments;
			return fragment;
		}

 		// TODO: add savedInstanceStat processe
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Chrono = new Stopwatch();
			Chrono.Start();

			Brands = MainDatabase.GetItems<DrugBrand>();

			SKUs = MainDatabase.GetItems<DrugSKU>();
			SKUsDict = new Dictionary<string, DrugSKU>(SKUs.Count);
			for (int s = 0; s < SKUs.Count; s++) {
				SKUsDict.Add(SKUs[s].uuid, SKUs[s]);	
			}

			DistributionDatasDict = new Dictionary<string, DistributionData>(SKUs.Count);

			WorkTypes = MainDatabase.GetItems<WorkType>();
			Promotions = MainDatabase.GetItems<Promotion>();
			PromotionsList = new List<Promotion>();
			PromotionsList.Add(new Promotion { name = @"Выберите акцию!", uuid = Guid.Empty.ToString() });
			PromotionsList.AddRange(Promotions);
			MessageTypes = MainDatabase.GetItems<MessageType>();

			var pharmacyUUID = Arguments.GetString(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return;

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);

			Agreements = new Dictionary<string, DistributionAgreement>();
			foreach (var agreement in MainDatabase.GetItems<DistributionAgreement>()
													  .Where(da => (da.object_type == "pharmacy")
																&& (da.object_uuid == Pharmacy.UUID)
															)
					) {
				if (!Agreements.ContainsKey(agreement.drugSKU)) {
					Agreements.Add(agreement.drugSKU, agreement);
				}
			}

			if (string.IsNullOrEmpty(Pharmacy.Net)) return;

			foreach (var agreement in MainDatabase.GetItems<DistributionAgreement>()
													  .Where(da => (da.object_type == "net")
																&& (da.object_uuid == Pharmacy.Net)
															)
					) {
				if (Agreements.ContainsKey(agreement.drugSKU)) {
					Agreements[agreement.drugSKU] = agreement;
				} else {
					Agreements.Add(agreement.drugSKU, agreement);
				}
			}
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var shared = Activity.GetSharedPreferences(MainActivity.C_MAIN_PREFS, Android.Content.FileCreationMode.Private);
			HOST_URL = shared.GetString(Dialogs.SigninDialog.C_HOST_URL, string.Empty);

			View mainView = inflater.Inflate(Resource.Layout.InfoFragment, container, false);

			var pharmacyUUID = Arguments.GetString(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return mainView;

			var attendanceLastUUID = Arguments.GetString(C_ATTENDANCE_LAST_UUID);
			var attendanceLast = string.IsNullOrEmpty(attendanceLastUUID) ? null : MainDatabase.GetEntity<Attendance>(attendanceLastUUID);

			var isAttendanceRunning = Arguments.GetBoolean(Consts.C_IS_ATTENDANCE_RUNNING, false);

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
			Employees = MainDatabase.GetEmployees(pharmacyUUID);
			
			Locker = mainView.FindViewById<TextView>(Resource.Id.locker);
			Arrow = mainView.FindViewById<ImageView>(Resource.Id.arrow);

			if (isAttendanceRunning) {
				Arrow.Visibility = ViewStates.Gone;
				Locker.Visibility = ViewStates.Gone;

				InitWhenRunning(attendanceLastUUID, mainView);
				return mainView;
			}

			//InitDistribution(mainView, attendanceLast);

			//InitAttendanceContent(mainView, attendanceLast);

			//InitPromotion(mainView, attendanceLast);

			//InitCompetitor(mainView, attendanceLast);

			//InitMessage(mainView, attendanceLast);

			//InitSaleTable(mainView, attendanceLast);

			//InitResume(mainView, attendanceLast);

			if (attendanceLast != null) {
				if (attendanceLast.When.Date == DateTimeOffset.UtcNow.Date) {
					Arrow.Visibility = ViewStates.Gone;
					Locker.Text = string.Empty;
				}
			}

			return mainView;
		}

		void InitWhenRunning(string attendanceLastUUID, View view = null)
		{
			var mainView = (view == null) ? View : view;

			// 1. Дистрибьюция
			if (DistributionDatasDict.Count == 0) {
				var distributions = MainDatabase.GetDistributions(attendanceLastUUID);
				if (distributions.Count == 0) {
					using (var transaction = MainDatabase.BeginTransaction()) {
						for (int s = 0; s < SKUs.Count; s++) {
							var distr = MainDatabase.CreateData<DistributionData>(attendanceLastUUID);
							distr.DrugSKU = SKUs[s].uuid;
							DistributionDatasDict.Add(distr.UUID, distr);
						}
						transaction.Commit();
					}
				} else {
					for (int d = 0; d < distributions.Count; d++) {
						DistributionDatasDict.Add(distributions[d].UUID, distributions[d]);
					}
				}
			}
			DistributionTable = mainView.FindViewById<TableLayout>(Resource.Id.ifDistributionTable);


			// 2. Содержание визит
			AttendanceTypeContent = mainView.FindViewById<ViewSwitcher>(Resource.Id.ifAtendanceTypeContentVS);
			AttendanceTypeContent.RemoveAllViews();

			// 2.1 Презентаци
			View presentationMainView = Activity.LayoutInflater.Inflate(
											Resource.Layout.InfoPresentationData,
											AttendanceTypeContent,
											false);
			presentationMainView.FindViewById<Button>(Resource.Id.ipdPresentationAddB).Click += (s, e) => {
				AddPresentationView();
			};
			PresentationTable = presentationMainView.FindViewById<LinearLayout>(Resource.Id.ipdPresentationTable);
			AddPresentationView();
			AttendanceTypeContent.AddView(presentationMainView);

			// 2.1 Фарм-кружо
			CoterieLayout = Activity.LayoutInflater.Inflate(
									Resource.Layout.InfoCoterieData,
									AttendanceTypeContent, false) as RelativeLayout;
			var coterieBrands = CoterieLayout.FindViewById<TextView>(Resource.Id.icdBrandsTV);
			coterieBrands.Click += CoterieBrands_Click;
			var coterieEmployees = CoterieLayout.FindViewById<TextView>(Resource.Id.icdEmployeesTV);
			coterieEmployees.Click += CoterieEmployees_Click;
			AttendanceTypeContent.AddView(CoterieLayout);

			AttendanceTypeContent.Visibility = ViewStates.Visible;
			var changeAttendanceType = mainView.FindViewById<Button>(Resource.Id.ifChangeAttendanceTypeB);
			changeAttendanceType.Visibility = ViewStates.Visible;
			changeAttendanceType.Click += (sender, e) => {
				AttendanceTypeContent.ShowNext();
			};

			// 3. Акц
			PromotionLayout = mainView.FindViewById<LinearLayout>(Resource.Id.ifPromotionLL);
			var promotionText = PromotionLayout.FindViewById<EditText>(Resource.Id.ifPromotionET);
			promotionText.Text = string.Empty;
			promotionText.Enabled = false;
			var promotionSpinner = PromotionLayout.FindViewById<Spinner>(Resource.Id.ifPromotionS);
			var promotionAdapter = new ArrayAdapter(
				Context,
				Android.Resource.Layout.SimpleSpinnerItem,
				PromotionsList.Select(x => x.name).ToArray()
			);
			promotionAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			promotionSpinner.Adapter = promotionAdapter;
			promotionSpinner.Clickable = true;
			promotionSpinner.ItemSelected += (sender, e) => {
				if (e.Position == 0) {
					promotionText.Text = string.Empty;
					promotionText.Enabled = false;
				} else {
					promotionText.Enabled = true;
				}
			};
			promotionSpinner.SetSelection(0); // Обнуление значений
			PromotionLayout.Visibility = ViewStates.Visible;
			mainView.FindViewById<View>(Resource.Id.ifPromotionDividerV).Visibility = ViewStates.Visible;

			// 4. Активность конкуренто
			CompetitorLayout = mainView.FindViewById<LinearLayout>(Resource.Id.ifCompetitorLL);
			var competitorText = CompetitorLayout.FindViewById<EditText>(Resource.Id.ifCompetitorET);
			var competitorCB = CompetitorLayout.FindViewById<CheckBox>(Resource.Id.ifCompetitorCB);
			competitorCB.Enabled = true;
			competitorCB.CheckedChange += (sender, e) => {
				if (e.IsChecked) {
					competitorText.Enabled = true;
				} else {
					competitorText.Text = string.Empty;
					competitorText.Enabled = false;
				}
			};
			competitorCB.Checked = false;
			CompetitorLayout.Visibility = ViewStates.Visible;
			mainView.FindViewById<View>(Resource.Id.ifCompetitorDividerV).Visibility = ViewStates.Visible;

			// 5. Сообщени
			var messageLayout = mainView.FindViewById<LinearLayout>(Resource.Id.ifMessageLL);
			var messageAdd = messageLayout.FindViewById<Button>(Resource.Id.ifMessageAddB);
			messageAdd.Visibility = ViewStates.Visible;
			messageAdd.Click += (s, e) => {
				AddMessageView();
			};
			var messageTable = messageLayout.FindViewById<LinearLayout>(Resource.Id.ifMessageTable);
			messageTable.RemoveAllViews();
			AddMessageView(messageTable);
			messageLayout.Visibility = ViewStates.Visible;
			var messageDivider = mainView.FindViewById<View>(Resource.Id.ifMessageDividerV);
			messageDivider.Visibility = ViewStates.Visible;

			//// 6. Продаж
			//for (int c = 2; c < SaleTable.ChildCount; c = c + 2) {
			//	for (int m = 0; m < SaleDataMonths.Count(); m++) {
			//		(SaleTable.GetChildAt(c) as LinearLayout).GetChildAt(m + 1).Enabled = true;
			//	}
			//}
			//SaleLayout.Visibility = ViewStates.Visible;
			//SaleDivider.Visibility = ViewStates.Visible;

			// 7. Резюме визит
			ResumeLayout = mainView.FindViewById<LinearLayout>(Resource.Id.ifResumeLL);
			var resumeText = ResumeLayout.FindViewById<EditText>(Resource.Id.ifResumeET);
			resumeText.Text = string.Empty;
			resumeText.Enabled = true;
			ResumeLayout.Visibility = ViewStates.Visible;
		}

		void RestoreViews()
		{
			{
				if (DistributionDatasDict.Count > 0) {

					DistributionTable.RemoveAllViews();
					Activity.LayoutInflater.Inflate(Resource.Layout.DistributionTableHeader, DistributionTable, true);
					foreach (var distribution in DistributionDatasDict.Values) {
						System.Diagnostics.Debug.WriteLine(string.Concat("distribution: ", distribution.UUID));
						//for (int d = 0; d < DistributionDatasDict.Count; d++) {
						var row = Activity.LayoutInflater.Inflate(Resource.Layout.DistributionTableItem, DistributionTable, false);
						row.SetTag(Resource.String.DistributionDataUUID, distribution.UUID);
						row.SetTag(Resource.String.DrugSKUUUID, distribution.DrugSKU);
						row.FindViewById<TextView>(Resource.Id.dtiDrugSKUTV).Text = SKUsDict[distribution.DrugSKU].name;

						var isExistence = row.FindViewById<CheckBox>(Resource.Id.dtiIsExistenceCB);
						isExistence.Enabled = true;
						isExistence.CheckedChange += (sender, e) => {
							var cb = (CheckBox)sender;
							// TODO: danger place!!!
							var tr = (TableRow)cb.Parent.Parent;
							tr.FindViewById<EditText>(Resource.Id.dtiCountET).Enabled = e.IsChecked;
							tr.FindViewById<EditText>(Resource.Id.dtiPriceET).Enabled = e.IsChecked;
							tr.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Enabled = e.IsChecked;
							tr.FindViewById<CheckBox>(Resource.Id.dtiHasPOSCB).Enabled = e.IsChecked;
							//ll.FindViewById<EditText>(Resource.Id.dtiOrderET).Enabled = e.IsChecked;
							//ll.FindViewById<EditText>(Resource.Id.dtiCommentET).Enabled = e.IsChecked;
						};
						isExistence.Checked = distribution.IsExistence;

						DistributionAgreement agreement = null;
						if (Agreements.ContainsKey(distribution.DrugSKU)) {
							agreement = Agreements[distribution.DrugSKU];
							row.FindViewById<EditText>(Resource.Id.dtiCountET).Hint = agreement.count.ToString();
							row.FindViewById<EditText>(Resource.Id.dtiPriceET).Hint = agreement.price.ToString();
							row.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked = agreement.isPresence;
							row.FindViewById<CheckBox>(Resource.Id.dtiHasPOSCB).Checked = agreement.hasPOS;
							row.FindViewById<EditText>(Resource.Id.dtiOrderET).Hint = agreement.order;
							row.FindViewById<EditText>(Resource.Id.dtiCommentET).Hint = agreement.comment;
						} else {
							row.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked = false;
							row.FindViewById<CheckBox>(Resource.Id.dtiHasPOSCB).Checked = false;
						}
						row.FindViewById<EditText>(Resource.Id.dtiCountET).Text = distribution.Count == 0 ? string.Empty : distribution.Count.ToString();
						row.FindViewById<EditText>(Resource.Id.dtiPriceET).Text = distribution.Price == 0 ? string.Empty : distribution.Price.ToString();
						row.FindViewById<EditText>(Resource.Id.dtiOrderET).Text = distribution.Order;
						row.FindViewById<EditText>(Resource.Id.dtiCommentET).Text = distribution.Comment;

						DistributionTable.AddView(row);}
				}
			}

			{
				if (PromotionData != null) {
					PromotionLayout.FindViewById<EditText>(Resource.Id.ifPromotionET).Text = PromotionData.Text;
					for (int p = 1; p < PromotionsList.Count; p++) {
						if (PromotionsList[p].uuid == PromotionData.Promotion) {
							PromotionLayout.FindViewById<Spinner>(Resource.Id.ifPromotionS).SetSelection(p);
							break;
						}
					}
				}
			}

			{
				if (CompetitorData != null) {
					CompetitorLayout.FindViewById<CheckBox>(Resource.Id.ifCompetitorCB).Checked = true;
					CompetitorLayout.FindViewById<EditText>(Resource.Id.ifCompetitorET).Text = CompetitorData.Text;
				}	
			}

			// 7. Резюме визита
			{
				if (ResumeData != null) {
					ResumeLayout.FindViewById<EditText>(Resource.Id.ifResumeET).Text = ResumeData.Text;
				}
			}
		}

		void AddPresentationView(View view = null)
		{
			var presentation = Activity.LayoutInflater.Inflate(Resource.Layout.InfoPresentationItem, PresentationTable, false);
			var employee = presentation.FindViewById<TextView>(Resource.Id.ipiEmployeeTV);
			employee.Click += (object sender, EventArgs e) => {
				var usedEmployees = new List<string>();
				for (int c = 0; c < PresentationTable.ChildCount; c++) {
					var row = PresentationTable.GetChildAt(c) as LinearLayout;
					var employeeUUID = (string) row.GetTag(Resource.String.PDEmployee);
					if (string.IsNullOrEmpty(employeeUUID)) continue;
					usedEmployees.Add(employeeUUID);
				}

				var availEmployees = Employees.Where(emp => !usedEmployees.Contains(emp.UUID)).ToList();

				new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите сотрудника аптеки:")
						   .SetCancelable(true)
						   .SetItems(
					           availEmployees.Select(item => item.Name).ToArray(),
					           (caller, arguments) => {
									presentation.SetTag(Resource.String.PDEmployee, availEmployees[arguments.Which].UUID);
									employee.Text = availEmployees[arguments.Which].Name;
								}
					          )
						   .Show();
			};


			var brandTable = presentation.FindViewById<LinearLayout>(Resource.Id.ipiBrandTable);

			var brandRow = Activity.LayoutInflater.Inflate(Resource.Layout.InfoPresentationSubItem, brandTable, false);

			brandRow.FindViewById<TextView>(Resource.Id.ipsiBrandTV).Click += Brand_Click;

			brandRow.FindViewById<TextView>(Resource.Id.ipsiWorkTypesTV).Click += WorkTypes_Click;

			brandTable.AddView(brandRow);

			PresentationTable.AddView(presentation);
		}

		void Brand_Click(object sender, EventArgs e)
		{
			var tv = (TextView)sender;
			var brandTable = tv.Parent.Parent as LinearLayout;

			var cacheBrands = new List<string>();
			for (int c = 0; c < brandTable.ChildCount; c++) {
				var row = brandTable.GetChildAt(c) as LinearLayout;
				var brandUUID = (string)row.GetTag(Resource.String.PDBrand);
				if (string.IsNullOrEmpty(brandUUID)) continue;
				cacheBrands.Add(brandUUID);
			}

			bool isCacheBrandsWasEmpty = cacheBrands.Count == 0;
			bool[] checkedItems = new bool[Brands.Count];
			if (!isCacheBrandsWasEmpty) {
				for (int i = 0; i < Brands.Count; i++) {
					checkedItems[i] = cacheBrands.Contains(Brands[i].uuid);
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
							   if (isCacheBrandsWasEmpty) {
								   brandTable.RemoveAllViews();

								   if (cacheBrands.Count > 0) {
									   foreach (var brandUUID in cacheBrands) {
										   var row = Activity.LayoutInflater.Inflate(Resource.Layout.InfoPresentationSubItem, brandTable, false);
										   row.SetTag(Resource.String.PDBrand, brandUUID);
										   row.FindViewById<TextView>(Resource.Id.ipsiBrandTV).Text = Brands.Single(b => b.uuid == brandUUID).name;
										   row.FindViewById<TextView>(Resource.Id.ipsiBrandTV).Click += Brand_Click;
										   row.FindViewById<TextView>(Resource.Id.ipsiWorkTypesTV).Click += WorkTypes_Click;
										   brandTable.AddView(row);
									   }
								   } else {
									   var emptyRow = Activity.LayoutInflater.Inflate(Resource.Layout.InfoPresentationSubItem, brandTable, false);
									   emptyRow.FindViewById<TextView>(Resource.Id.ipsiBrandTV).Click += Brand_Click;
									   emptyRow.FindViewById<TextView>(Resource.Id.ipsiWorkTypesTV).Click += WorkTypes_Click;
									   brandTable.AddView(emptyRow);
								   }
							   } else {
								   if (cacheBrands.Count > 0) {
									   var viewsWhichDelete = new List<View>();
									    for (int c = 0; c < brandTable.ChildCount; c++) {
											var row = brandTable.GetChildAt(c);
										    var brandUUID = (string)row.GetTag(Resource.String.PDBrand);
										  	if (string.IsNullOrEmpty(brandUUID)) continue;
										   	bool isExists = false;
										    foreach (var item in cacheBrands) {
												if (brandUUID == item) {
											   		cacheBrands.Remove(item);
											   		isExists = true;
											   		break;
												}
											}
										   if (!isExists) {
											   viewsWhichDelete.Add(row);
										   }
									    }
										
										foreach (var view in viewsWhichDelete) {
											brandTable.RemoveView(view);
									   }

									   foreach (var brandUUID in cacheBrands) {
											var row = Activity.LayoutInflater.Inflate(Resource.Layout.InfoPresentationSubItem, brandTable, false);
										    row.SetTag(Resource.String.PDBrand, brandUUID);
										    row.FindViewById<TextView>(Resource.Id.ipsiBrandTV).Text = Brands.Single(b => b.uuid == brandUUID).name;
										    row.FindViewById<TextView>(Resource.Id.ipsiBrandTV).Click += Brand_Click;
										    row.FindViewById<TextView>(Resource.Id.ipsiWorkTypesTV).Click += WorkTypes_Click;
										    brandTable.AddView(row);
									    }

								   } else {
									    brandTable.RemoveAllViews();
									    var emptyRow = Activity.LayoutInflater.Inflate(Resource.Layout.InfoPresentationSubItem, brandTable, false);
										emptyRow.FindViewById<TextView>(Resource.Id.ipsiBrandTV).Click += Brand_Click;
									    emptyRow.FindViewById<TextView>(Resource.Id.ipsiWorkTypesTV).Click += WorkTypes_Click;
									    brandTable.AddView(emptyRow);
								   }
							   }
								(caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton(@"Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
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

		void InitDistribution(View view, Attendance attendanceLast)
		{
			//DistributionTable = view.FindViewById<LinearLayout>(Resource.Id.ifDistributionTable);

			//View header = Activity.LayoutInflater.Inflate(Resource.Layout.DistributionTableHeader, DistributionTable, false);
			////TODO: DEBUG
			//if (HOST_URL.Contains("johnson")) {
			//	header.FindViewById<TextView>(Resource.Id.dthCommentTV).Text = "Приход";
			//}

			//View divider = Activity.LayoutInflater.Inflate(Resource.Layout.Divider, DistributionTable, false);

			//DistributionTable.AddView(header);
			//DistributionTable.AddView(divider);

			//if (attendanceLast == null) {
			//	foreach (var SKU in SKUs) {
			//		var row = (Activity.LayoutInflater.Inflate(
			//							Resource.Layout.DistributionTableItem,
			//							DistributionTable,
			//							false)
			//				  ) as LinearLayout;

			//		row.SetTag(Resource.String.DrugSKUUUID, SKU.uuid);
			//		row.FindViewById<TextView>(Resource.Id.dtiDrugSKUTV).Text = SKU.name;

			//		if (HOST_URL.Contains("johnson")) {
			//			row.FindViewById<TextView>(Resource.Id.dtiCommentET).Hint = "Приход";
			//		}

			//		DistributionTable.AddView(row);

			//		divider = Activity.LayoutInflater.Inflate(Resource.Layout.Divider, DistributionTable, false);

			//		DistributionTable.AddView(divider);
			//	}
			//} else {
				//var distributions = MainDatabase.GetDistributions(attendanceLast.UUID);
				//var dictDistrs = new Dictionary<string, DistributionData>();
				//foreach (var item in distributions) {
				//	dictDistrs.Add(item.DrugSKU, item); 
				//}

				//foreach (var SKU in SKUs) {
				//	var row = (Activity.LayoutInflater.Inflate(
				//						Resource.Layout.DistributionTableItem,
				//						DistributionTable,
				//						false)
				//			  ) as LinearLayout;

				//	var item = dictDistrs[SKU.uuid];
				//	row.SetTag(Resource.String.DrugSKUUUID, SKU.uuid);
				//	row.FindViewById<TextView>(Resource.Id.dtiDrugSKUTV).Text = SKU.name;
				//	row.FindViewById<CheckBox>(Resource.Id.dtiIsExistenceCB).Checked = item.IsExistence;
				//	row.FindViewById<EditText>(Resource.Id.dtiCountET).Text = item.Count.ToString();
				//	row.FindViewById<EditText>(Resource.Id.dtiPriceET).Text = item.Price.ToString();
				//	row.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked = item.IsPresence;
				//	row.FindViewById<CheckBox>(Resource.Id.dtiHasPOSCB).Checked = item.HasPOS;
				//	row.FindViewById<EditText>(Resource.Id.dtiOrderET).Text = item.Order;
				//	row.FindViewById<EditText>(Resource.Id.dtiCommentET).Text = item.Comment;

				//	if (HOST_URL.Contains("johnson")) {
				//		row.FindViewById<TextView>(Resource.Id.dtiCommentET).Hint = "Приход";
				//	}

				//	DistributionTable.AddView(row);

				//	divider = Activity.LayoutInflater.Inflate(Resource.Layout.Divider, DistributionTable, false);

				//	DistributionTable.AddView(divider);
				//}
			//}
		}

		void InitAttendanceContent(View view, Attendance attendanceLast)
		{
			var attendanceTypeContent = view.FindViewById<ViewSwitcher>(Resource.Id.ifAtendanceTypeContentVS);
			attendanceTypeContent.SetInAnimation(Activity, Android.Resource.Animation.SlideInLeft);
			attendanceTypeContent.SetOutAnimation(Activity, Android.Resource.Animation.SlideOutRight);

			//ChangeAttendanceType = view.FindViewById<Button>(Resource.Id.ifChangeAttendanceTypeB);
			//ChangeAttendanceType.Click += (sender, e) => {
			//	AttendanceTypeContent.ShowNext();
			//};
			//ChangeAttendanceType.Visibility = ViewStates.Gone;

			//if (attendanceLast == null) {
			//	AttendanceTypeContent.Visibility = ViewStates.Gone;
			//	return;
			//}

			//var presentationsGrouped = MainDatabase.GetGroupedPresentationDatas(attendanceLast.UUID);
			//if (presentationsGrouped.Count > 0) {
			//	View presentationMainView = Activity.LayoutInflater.Inflate(
			//									Resource.Layout.InfoPresentationData,
			//									AttendanceTypeContent, 
			//									false);
				
			//	var addView = presentationMainView.FindViewById<Button>(Resource.Id.ipdPresentationAddB);
			//	addView.Visibility = ViewStates.Gone;

			//	PresentationTable = presentationMainView.FindViewById<LinearLayout>(Resource.Id.ipdPresentationTable);

			//	foreach (var employee in presentationsGrouped) {
			//		var presentationItem = Activity.LayoutInflater.Inflate(Resource.Layout.InfoPresentationItem, PresentationTable, false);
			//		presentationItem.FindViewById<TextView>(Resource.Id.ipiEmployeeTV).Text = 
			//			MainDatabase.GetEntity<Employee>(employee.Key).Name;
			//		var brandTable = presentationItem.FindViewById<LinearLayout>(Resource.Id.ipiBrandTable);

			//		foreach (var brand in employee.Value) {
			//			var brandRow = Activity.LayoutInflater.Inflate(Resource.Layout.InfoPresentationSubItem, brandTable, false);
			//			brandRow.FindViewById<TextView>(Resource.Id.ipsiBrandTV).Text =
			//				        string.IsNullOrEmpty(brand.Key) ? @"<пусто>" : MainDatabase.GetItem<DrugBrand>(brand.Key).name;
			//			brandRow.FindViewById<TextView>(Resource.Id.ipsiWorkTypesTV).Text =
			//				        string.Join(System.Environment.NewLine, brand.Value.Select(wt => wt.name));
			//			brandTable.AddView(brandRow);
			//		}

			//		PresentationTable.AddView(presentationItem);
			//	}

			//	AttendanceTypeContent.AddView(presentationMainView);

			//	return;
			//}

			//var coterieDataGrouped = MainDatabase.GetCoterieDataGrouped(attendanceLast.UUID);

			//if (coterieDataGrouped.Brands.Count > 0 || coterieDataGrouped.Employees.Count > 0) {
			//	CoterieLayout = Activity.LayoutInflater.Inflate(
			//							Resource.Layout.InfoCoterieData,
			//							AttendanceTypeContent,
			//							false) as LinearLayout;
			//	string brands = string.Join(System.Environment.NewLine, coterieDataGrouped.Brands.Values.Select(b => b.name).ToArray());
			//	CoterieLayout.FindViewById<TextView>(Resource.Id.icdBrandsTV).Text = brands;
			//	string employees = string.Join(System.Environment.NewLine, coterieDataGrouped.Employees.Values.Select(e => e.Name).ToArray());
			//	CoterieLayout.FindViewById<TextView>(Resource.Id.icdEmployeesTV).Text = employees;

			//	AttendanceTypeContent.AddView(CoterieLayout);

			//	return;
			//}

			// если ничего нет, то спрятать
			attendanceTypeContent.Visibility = ViewStates.Gone;
			//ChangeAttendanceType.Visibility = ViewStates.Gone;
		}

		//void InitPromotion(View view, Attendance attendanceLast)
		//{
		//	PromotionLayout = view.FindViewById<LinearLayout>(Resource.Id.ifPromotionLL);
		//	PromotionDivider = view.FindViewById<View>(Resource.Id.ifPromotionDividerV);

		//	var promotionText = view.FindViewById<EditText>(Resource.Id.ifPromotionET); 

		//	var promotionsList = new List<Promotion>();
		//	promotionsList.Add(new Promotion { name = @"Выберите акцию!", uuid = Guid.Empty.ToString() });
		//	promotionsList.AddRange(Promotions);
		//	var promotionSpinner = view.FindViewById<Spinner>(Resource.Id.ifPromotionS);
		//	var promotionAdapter = new ArrayAdapter(
		//		Context,
		//		Android.Resource.Layout.SimpleSpinnerItem,
		//		promotionsList.Select(x => x.name).ToArray()
		//	);
		//	promotionAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
		//	promotionSpinner.Adapter = promotionAdapter;

		//	if (attendanceLast == null) {
		//		PromotionLayout.Visibility = ViewStates.Gone;
		//		PromotionDivider.Visibility = ViewStates.Gone;
		//		return;
		//	}

		//	var promotionData = MainDatabase.GetSingleData<PromotionData>(attendanceLast.UUID);

		//	if (promotionData == null) {
		//		PromotionLayout.Visibility = ViewStates.Gone;
		//		PromotionDivider.Visibility = ViewStates.Gone;
		//		return;
		//	}

		//	promotionText.Text = promotionData.Text;
		//	for (int p = 1; p < promotionsList.Count; p++) {
		//		if (promotionsList[p].uuid == promotionData.Promotion) {
		//			promotionSpinner.SetSelection(p);
		//			break;
		//		}	
		//	}
		//}

		//void InitCompetitor(View view, Attendance attendanceLast)
		//{
		//	CompetitorLayout = view.FindViewById<LinearLayout>(Resource.Id.ifCompetitorLL);
		//	CompetitorDivider = view.FindViewById<View>(Resource.Id.ifCompetitorDividerV);

		//	if (attendanceLast == null) {
		//		CompetitorLayout.Visibility = ViewStates.Gone;
		//		CompetitorDivider.Visibility = ViewStates.Gone;
		//		return;
		//	}

		//	var competitorData = MainDatabase.GetSingleData<CompetitorData>(attendanceLast.UUID);

		//	if (competitorData == null) {
		//		CompetitorLayout.Visibility = ViewStates.Gone;
		//		CompetitorDivider.Visibility = ViewStates.Gone;
		//		return;
		//	}

		//	CompetitorLayout.FindViewById<EditText>(Resource.Id.ifCompetitorET).Text = competitorData.Text;
		//	CompetitorLayout.FindViewById<CheckBox>(Resource.Id.ifCompetitorCB).Checked = true;	
		//}

		//void InitMessage(View view, Attendance attendanceLast)
		//{
		//	MessageLayout = view.FindViewById<LinearLayout>(Resource.Id.ifMessageLL);
		//	MessageDivider = view.FindViewById<View>(Resource.Id.ifMessageDividerV);
		//	MessageTable = view.FindViewById<LinearLayout>(Resource.Id.ifMessageTable);
		//	var messageAdd = view.FindViewById<Button>(Resource.Id.ifMessageAddB);
		//	messageAdd.Visibility = ViewStates.Gone;

		//	if (attendanceLast == null) {
		//		MessageLayout.Visibility = ViewStates.Gone;
		//		MessageDivider.Visibility = ViewStates.Gone;
		//		return;
		//	}

		//	var messages = MainDatabase.GetDatas<MessageData>(attendanceLast.UUID);

		//	if (messages.Count == 0) {
		//		MessageLayout.Visibility = ViewStates.Gone;
		//		MessageDivider.Visibility = ViewStates.Gone;
		//		return;
		//	}


		//	var messageTypes = new List<MessageType>();
		//	messageTypes.Add(new MessageType { name = @"Выберите тип сообщения!", uuid = Guid.Empty.ToString() });
		//	messageTypes.AddRange(MessageTypes);

		//	foreach (var item in messages) {
		//		var row = Activity.LayoutInflater.Inflate(Resource.Layout.InfoMessageItem, MessageTable, false);
		//		row.FindViewById<EditText>(Resource.Id.imiMessageTextET).Text = item.Text;
		//		var type = row.FindViewById<Spinner>(Resource.Id.imiMessageTypeS);
		//		type.Adapter = new ArrayAdapter(
		//			Context,
		//			Android.Resource.Layout.SimpleSpinnerItem,
		//			messageTypes.Select(x => x.name).ToArray()
		//		);

		//		for (int mt = 1; mt < messageTypes.Count; mt++) {
		//			if (messageTypes[mt].uuid == item.Type) {
		//				type.SetSelection(mt);
		//				break;
		//			}
		//		}

		//		MessageTable.AddView(row);
		//	}
		//}

		//void InitSaleTable(View view, Attendance attendanceLast)
		//{
		//	SaleLayout = view.FindViewById<LinearLayout>(Resource.Id.ifSaleLL);
		//	SaleDivider = view.FindViewById<View>(Resource.Id.ifSaleDividerV);
		//	SaleTable = SaleLayout.FindViewById<LinearLayout>(Resource.Id.ifSaleTable);

		//	var header = (LinearLayout)Activity.LayoutInflater.Inflate(Resource.Layout.SaleTableHeader, SaleTable, false);
		//	SaleDataMonths = new DateTimeOffset[8];
		//	foreach (var SKU in SKUs) {
		//		for (int m = 0; m < 8; m++) {
		//			var month = DateTimeOffset.Now.AddMonths(-6 + m);
		//			SaleDataMonths[m] = month;
		//			var hView = header.GetChildAt(m + 1);
		//			if (hView is TextView) {
		//				(hView as TextView).Text = month.ToString(string.Format(@"MMMM{0}yyyy", System.Environment.NewLine));
		//			}
		//		}
		//	}

		//	View divider = Activity.LayoutInflater.Inflate(Resource.Layout.Divider, SaleTable, false);

		//	SaleTable.AddView(header);
		//	SaleTable.AddView(divider);

		//	var key = string.Empty;
		//	var formatForKey = @"MMyyyy";
		//	SaleDataTextViews = new Dictionary<string, TextView>();
		//	foreach (var SKU in SKUs) {
		//		var row = (Activity.LayoutInflater.Inflate(
		//							Resource.Layout.SaleTableItem,
		//							SaleTable,
		//							false)) as LinearLayout;
		//		row.SetTag(Resource.String.DrugSKUUUID, SKU.uuid);
		//		row.FindViewById<TextView>(Resource.Id.stiDrugSKUTV).Text = SKU.name;

		//		for (int c = 1; c < row.ChildCount; c++) {
		//			var rView = (EditText)row.GetChildAt(c);
		//			rView.SetTag(Resource.String.IsChanged, false);
		//			rView.AfterTextChanged += RView_AfterTextChanged;
		//			rView.SetFilters(new IInputFilter[] { new DecimalPlaceFilter(3, 1) });

		//			key = string.Format("{0}-{1}", SKU.uuid, SaleDataMonths[c - 1].ToString(formatForKey));
		//			SaleDataTextViews.Add(key, rView);
		//		}

		//		SaleTable.AddView(row);

		//		divider = Activity.LayoutInflater.Inflate(Resource.Layout.Divider, SaleTable, false);

		//		SaleTable.AddView(divider);
		//	}

		//	var saleDatas = MainDatabase.GetSaleDatas(Pharmacy.UUID, SaleDataMonths);

		//	if (saleDatas.Count() == 0) {
		//		SaleLayout.Visibility = ViewStates.Gone;
		//		SaleDivider.Visibility = ViewStates.Gone;
		//		return;
		//	}

		//	TextView txt;
		//	foreach (var sale in saleDatas) {
		//		//key = string.Format("{0}-{1}", sale.DrugSKU, sale.Month.ToString(formatForKey));
		//		key = string.Format("{0}-{1}{2}", sale.DrugSKU, sale.Month.ToString("D2"), sale.Year);
		//		if (SaleDataTextViews.ContainsKey(key)) {
		//			txt = SaleDataTextViews[key];
		//			txt.SetTag(Resource.String.SaleDataUUID, sale.UUID);
		//			txt.Text = sale.Sale == null ? string.Empty : sale.Sale.ToString();
		//		}
		//	}
		//}

		//void InitResume(View view, Attendance attendanceLast)
		//{
		//	ResumeLayout = view.FindViewById<LinearLayout>(Resource.Id.ifResumeLL);
		//	if (attendanceLast == null) {
		//		ResumeLayout.Visibility = ViewStates.Gone;
		//		return;
		//	}

		//	var resumeData = MainDatabase.GetSingleData<ResumeData>(attendanceLast.UUID);

		//	if (resumeData == null) {
		//		ResumeLayout.Visibility = ViewStates.Gone;
		//		return;
		//	}

		//	ResumeLayout.FindViewById<EditText>(Resource.Id.ifResumeET).Text = resumeData.Text;
		//}

		void RView_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
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

			}
			//else if (AttendanceStart == null) {
			//	Locker.Visibility = ViewStates.Visible;
			//

			RestoreViews();

			string debug = string.Concat(AttendanceActivity.C_TAG_FOR_DEBUG, "-", "InfoFragment", ":", Chrono.ElapsedMilliseconds);
			System.Diagnostics.Debug.WriteLine(debug);
		}

		public override void OnPause()
		{
			base.OnPause();
			var isAttendanceRunning = Arguments.GetBoolean(Consts.C_IS_ATTENDANCE_RUNNING, false);
			if (isAttendanceRunning) {
				SaveData();
			}
		}

		void SaveData()
		{
			Stopwatch chrono = new Stopwatch();
			chrono.Start();
			var attendanceLastUUID = Arguments.GetString(C_ATTENDANCE_LAST_UUID, string.Empty);

			using (var transaction = MainDatabase.BeginTransaction()) {
				// 1. Дистрибьюция
				{
					//var distributionTable = View.FindViewById<TableLayout>(Resource.Id.ifDistributionTable);
					for (int c = 1; c < DistributionTable.ChildCount; c++) {
						var row = DistributionTable.GetChildAt(c);
						var distrUUID = (string)row.GetTag(Resource.String.DistributionDataUUID);
						var drugSKU = (string)row.GetTag(Resource.String.DrugSKUUUID);
						//var distr = MainDatabase.CreateData<DistributionData>(attendanceLastUUID);
						var distr = DistributionDatasDict[distrUUID];
						distr.DrugSKU = drugSKU;
						distr.IsExistence = row.FindViewById<CheckBox>(Resource.Id.dtiIsExistenceCB).Checked;
						distr.Count = Helper.ToIntExeptNull(row.FindViewById<EditText>(Resource.Id.dtiCountET).Text);
						distr.Price = Helper.ToIntExeptNull(row.FindViewById<EditText>(Resource.Id.dtiPriceET).Text);
						distr.IsPresence = row.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked;
						distr.HasPOS = row.FindViewById<CheckBox>(Resource.Id.dtiHasPOSCB).Checked;
						distr.Order = row.FindViewById<EditText>(Resource.Id.dtiOrderET).Text;
						distr.Comment = row.FindViewById<EditText>(Resource.Id.dtiCommentET).Text;
					}
					//for (int d = 0; d < Distributions.Count; d++) {
					//	MainDatabase.DeleteEntity(transaction, Distributions[d]);	
					//}
				}
				           
				/////////////////////////
				// 2. Содержание визита
				/////////////////////////

				// 2.1 Презентаци
				{
					var presentationDatas = MainDatabase.GetDatas<PresentationData>(attendanceLastUUID);
					var presentationDatasDict = new Dictionary<string, PresentationData>(presentationDatas.Count);
					for (int pd = 0; pd < presentationDatas.Count; pd++) {
						presentationDatasDict.Add(presentationDatas[pd].GetComplexKey(), presentationDatas[pd]);
					}
					var presentationTable = View.FindViewById<LinearLayout>(Resource.Id.ipdPresentationTable);
					for (int c = 0; c < presentationTable.ChildCount; c++) {
						var row = presentationTable.GetChildAt(c);
						var employeeUUID = (string)row.GetTag(Resource.String.PDEmployee);
						if (string.IsNullOrEmpty(employeeUUID)) continue;

						var brandTable = row.FindViewById<LinearLayout>(Resource.Id.ipiBrandTable);
						for (int cc = 0; cc < brandTable.ChildCount; cc++) {
							var subRow = (LinearLayout)brandTable.GetChildAt(cc);
							var brandUUID = (string)subRow.GetTag(Resource.String.PDBrand);
							if (string.IsNullOrEmpty(brandUUID)) continue;

							var workTypes = (string)subRow.FindViewById<TextView>(Resource.Id.ipsiWorkTypesTV).GetTag(Resource.String.PDWorkTypes);
							if (string.IsNullOrEmpty(workTypes)) continue;

							foreach (var workType in workTypes.Split(';')) {
								if (presentationDatasDict.ContainsKey(string.Concat(employeeUUID, ':', brandUUID, ':', workType))) continue;
								var presentationData = MainDatabase.CreateData<PresentationData>(attendanceLastUUID);
								presentationData.Employee = employeeUUID;
								presentationData.Brand = brandUUID;
								presentationData.WorkType = workType;
							}
						}
					}
				}
				// 2.1 Фарм-кружок
				{
					//var coterieDatas = MainDatabase.GetDatas<CoterieData>(attendanceLastUUID);
					//var coterieDatasDict = new Dictionary<string, CoterieData>(coterieDatas.Count);
					//for (int cd = 0; cd < coterieDatas.Count; cd++) {
					//	coterieDatasDict.Add(coterieDatas[cd].GetComplexKey(), coterieDatas[cd]);
					//}
					//var coterieLayout = View.FindViewById<LinearLayout>(Resource.Id.icdCoterieDataLL);
					//var coterieBrands = coterieLayout.FindViewById<TextView>(Resource.Id.icdBrandsTV);
					//var brands = (string)coterieBrands.GetTag(Resource.String.CDBrands);
					//var coterieEmployees = coterieLayout.FindViewById<TextView>(Resource.Id.icdEmployeesTV);
					//var employees = (string)coterieEmployees.GetTag(Resource.String.CDEmployees);
					//if (!string.IsNullOrEmpty(brands) && !string.IsNullOrEmpty(employees)) {
					//	foreach (var brand in brands.Split(';')) {
					//		foreach (var employee in employees.Split(';')) {
					//			if (coterieDatasDict.ContainsKey(string.Concat(employee, ':', brand))) continue;

					//			var coterieData = MainDatabase.CreateData<CoterieData>(attendanceLastUUID);
					//			coterieData.Employee = employee;
					//			coterieData.Brand = brand;
					//		}
					//	}
					//}
				}
				/////////////////////////
				// !Содержание визита
				/////////////////////////

				// 3. Акция
				{
					var promotionSpinner = PromotionLayout.FindViewById<Spinner>(Resource.Id.ifPromotionS);
					if (promotionSpinner.SelectedItemPosition > 0) {
						if (PromotionData == null) {
							PromotionData = MainDatabase.CreateData<PromotionData>(attendanceLastUUID);
						}
						PromotionData.Promotion = Promotions[promotionSpinner.SelectedItemPosition - 1].uuid;
						PromotionData.Text = PromotionLayout.FindViewById<EditText>(Resource.Id.ifPromotionET).Text;
					} else if (PromotionData != null) {
						MainDatabase.DeleteEntity(transaction, PromotionData);
						PromotionData = null;
					}
				}

				// 4. Активность конкурентов
				{
					if (CompetitorLayout.FindViewById<CheckBox>(Resource.Id.ifCompetitorCB).Checked) {
						if (CompetitorData == null) {
							CompetitorData = MainDatabase.CreateData<CompetitorData>(attendanceLastUUID);
						}
						CompetitorData.Text = CompetitorLayout.FindViewById<EditText>(Resource.Id.ifCompetitorET).Text;
					} else if (CompetitorData != null) {
						MainDatabase.DeleteEntity(transaction, CompetitorData);
						CompetitorData = null;
					}
				}


				//// 5. Сообщения
				//{
				//	var messageDatas = MainDatabase.GetDatas<MessageData>(attendanceLastUUID);
				//	var messageDatasDict = new Dictionary<string, MessageData>(messageDatas.Count);
				//	for (int md = 0; md < messageDatas.Count; md++) {
				//		messageDatasDict.Add(messageDatas[md].Type, messageDatas[md]);
				//	}				
				//	var messageTable = View.FindViewById<LinearLayout>(Resource.Id.ifMessageTable);
				//	for (int c = 0; c < messageTable.ChildCount; c++) {
				//		var row = (LinearLayout)messageTable.GetChildAt(c);
				//		var messageDataUUID = (string)row.GetTag(Resource.String.MessageDataUUID);
				//		var messageTypeSpinner = row.FindViewById<Spinner>(Resource.Id.imiMessageTypeS);
				//		if (messageTypeSpinner.SelectedItemPosition > 0) {
				//			MessageData messageData;
				//			if (string.IsNullOrEmpty(messageDataUUID)) {
				//				messageData = MainDatabase.CreateData<MessageData>(attendanceLastUUID);
				//			} else {
				//				messageData = MainDatabase.GetEntity<MessageData>(messageDataUUID);
				//			}
				//			messageData.Type = MessageTypes[messageTypeSpinner.SelectedItemPosition - 1].uuid;
				//			messageData.Text = row.FindViewById<EditText>(Resource.Id.imiMessageTextET).Text;
				//		} else if (!string.IsNullOrEmpty(messageDataUUID)) {
				//			MainDatabase.DeleteEntity<PromotionData>(messageDataUUID);
				//		}
				//	}
				//}

				// 6. Продажи
				//for (int c = 2; c < SaleTable.ChildCount; c = c + 2) {
				//	var row = (LinearLayout)SaleTable.GetChildAt(c);
				//	var skuUUID = (string)row.GetTag(Resource.String.DrugSKUUUID);
				//	for (int m = 0; m < SaleDataMonths.Count(); m++) {
				//		var rView = (TextView)row.GetChildAt(m + 1);
				//		if (string.IsNullOrEmpty(rView.Text)) continue;

				//		var isChanged = (bool)rView.GetTag(Resource.String.IsChanged);
				//		if (isChanged) {
				//			var saleUUID = (string)rView.GetTag(Resource.String.SaleDataUUID);

				//			Console.WriteLine("Info: sku = {0}, value = {1}, sale = {2}", skuUUID, Helper.ToFloat(rView.Text), saleUUID);

				//			if (string.IsNullOrEmpty(saleUUID)) {
				//				var sale = MainDatabase.CreateData<SaleDataByMonth>(current.UUID);
				//				sale.Pharmacy = Pharmacy.UUID;
				//				sale.DrugSKU = skuUUID;
				//				sale.Year = SaleDataMonths[m].Year;
				//				sale.Month = SaleDataMonths[m].Month;
				//				sale.Sale = Helper.ToFloat(rView.Text);
				//			} else {
				//				var sale = MainDatabase.GetEntity<SaleDataByMonth>(saleUUID);
				//				Console.WriteLine("Info: sku = {0}, month = {1}", sale.DrugSKU == skuUUID, sale.Month == SaleDataMonths[m].Month);
				//				sale.Sale = Helper.ToFloat(rView.Text);
				//			}
				//		}
				//	}
				//}

				//// 6.1 Собираем данные в кварталы
				//var calcQuraters = new Stopwatch();
				//calcQuraters.Start();
				//var datas = MainDatabase.GetPharmacyDatas<SaleDataByMonth>(Pharmacy.UUID);
				//var dict = new Dictionary<string, Dictionary<int, List<SaleDataByMonth>[]>>();
				//foreach (var item in datas) {
				//	if (dict.ContainsKey(item.DrugSKU)) {
				//		if (dict[item.DrugSKU].ContainsKey(item.Year)) {
				//			dict[item.DrugSKU][item.Year][(item.Month - 1) / 3].Add(item);
				//		} else {
				//			dict[item.DrugSKU].Add(item.Year, new List<SaleDataByMonth>[4]);
				//			dict[item.DrugSKU][item.Year][0] = new List<SaleDataByMonth>();
				//			dict[item.DrugSKU][item.Year][1] = new List<SaleDataByMonth>();
				//			dict[item.DrugSKU][item.Year][2] = new List<SaleDataByMonth>();
				//			dict[item.DrugSKU][item.Year][3] = new List<SaleDataByMonth>();
				//			dict[item.DrugSKU][item.Year][(item.Month - 1) / 3].Add(item);
				//}
				//	} else {
				//		dict.Add(item.DrugSKU, new Dictionary<int, List<SaleDataByMonth>[]>());
				//		dict[item.DrugSKU].Add(item.Year, new List<SaleDataByMonth>[4]);
				//		dict[item.DrugSKU][item.Year][0] = new List<SaleDataByMonth>();
				//		dict[item.DrugSKU][item.Year][1] = new List<SaleDataByMonth>();
				//		dict[item.DrugSKU][item.Year][2] = new List<SaleDataByMonth>();
				//		dict[item.DrugSKU][item.Year][3] = new List<SaleDataByMonth>();
				//		dict[item.DrugSKU][item.Year][(item.Month - 1) / 3].Add(item);
				//	}
				//}

				//var oldQuarters = MainDatabase.GetPharmacyDatas<SaleDataByQuarter>(Pharmacy.UUID);
				//int newQuarters = 0;
				//foreach (var sku in dict) {
				//	foreach (var year in sku.Value) {
				//		for (int q = 1; q <= 4; q++) {
				//			if (year.Value[q - 1].Count == 3) {
				//				newQuarters++;
				//				var oldQuarter = oldQuarters.SingleOrDefault(oq => oq.DrugSKU == sku.Key && oq.Year == year.Key && oq.Quarter == q);
				//				if (oldQuarter == null) {
				//					var quarter = MainDatabase.Create2<SaleDataByQuarter>();
				//					quarter.Attendance = current.UUID;
				//					quarter.Pharmacy = Pharmacy.UUID;
				//					quarter.DrugSKU = sku.Key;
				//					quarter.Year = year.Key;
				//					quarter.Quarter = q;
				//					quarter.Sale = 0.0f;
				//					foreach (var item in year.Value[q - 1]) {
				//						quarter.Sale += item.Sale;
				//					}
				//					if (quarter.Sale.HasValue) {
				//						if (Math.Abs(quarter.Sale.Value) < 0.01) quarter.Sale = null;
				//					}
				//				} else {
				//					oldQuarter.Sale = 0.0f;
				//					foreach (var item in year.Value[q - 1]) {
				//						oldQuarter.Sale += item.Sale;
				//					}
				//					if (oldQuarter.Sale.HasValue) {
				//						if (Math.Abs(oldQuarter.Sale.Value) < 0.01) oldQuarter.Sale = null;
				//					}
				//				}
				//			}
				//		}
				//	}
				//}
				//calcQuraters.Stop();
				//Console.WriteLine(@"Calc: {0}, Count: {1}, oldQuarters: {2}, newQuarters: {3}", calcQuraters.ElapsedMilliseconds, datas.Count, oldQuarters.Count, newQuarters);


				// 7. Резюме визита
				{
					var resume = ResumeLayout.FindViewById<EditText>(Resource.Id.ifResumeET);
					if (!string.IsNullOrEmpty(resume.Text)) {
						if (ResumeData == null) {
							ResumeData = MainDatabase.CreateData<ResumeData>(attendanceLastUUID);
						}
						ResumeData.Text = resume.Text;
					} else if (ResumeData != null) {
						MainDatabase.DeleteEntity(transaction, ResumeData);
						ResumeData = null;
					}
				}

				transaction.Commit();
			}
			Console.WriteLine(@"SaveData: {0}", chrono.ElapsedMilliseconds);
		}

		public override void OnStop()
		{
			base.OnStop();
			DistributionTable = null;
			AttendanceTypeContent = null;
			PresentationTable = null;
			PromotionLayout = null;
			CompetitorLayout = null; ;
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

		void AddMessageView(View view = null)
		{
			var mainView = (view == null) ? View : view;

			var messageTable =mainView.FindViewById<LinearLayout>(Resource.Id.ifMessageTable);
			var message = Activity.LayoutInflater.Inflate(Resource.Layout.InfoMessageItem, messageTable, false);

			var messageText = message.FindViewById<EditText>(Resource.Id.imiMessageTextET);

			var messageTypes = new List<MessageType>();
			messageTypes.Add(new MessageType { name = @"Выберите тип сообщения!", uuid = Guid.Empty.ToString() });
			messageTypes.AddRange(MessageTypes);

			var messageSpinner = message.FindViewById<Spinner>(Resource.Id.imiMessageTypeS);
			var messageTypeAdapter = new ArrayAdapter(
				Context,
				Android.Resource.Layout.SimpleSpinnerItem,
				messageTypes.Select(x => x.name).ToArray()
			);
			messageTypeAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			messageSpinner.Adapter = messageTypeAdapter;
			messageSpinner.ItemSelected += (caller, arguments) => {
				if (arguments.Position == 0) {
					messageText.Text = string.Empty;
					messageText.Enabled = false;
				} else {
					messageText.Enabled = true;
				}
			};
			messageSpinner.Clickable = true;

			messageTable.AddView(message);
		}

		public void RefreshEmployees()
		{
			Employees = MainDatabase.GetEmployees(Pharmacy.UUID);
		}

		public void OnAttendanceStart(Attendance current)
		{
			AttendanceStart = current.When;
			Arrow.Visibility = ViewStates.Gone;
			Locker.Visibility = ViewStates.Gone;

			Arguments.PutString(C_ATTENDANCE_LAST_UUID, current.UUID);
			Arguments.PutBoolean(Consts.C_IS_ATTENDANCE_RUNNING, true);

			if (View == null) return;

			InitWhenRunning(current.UUID);
			RestoreViews();
		}

		public void OnAttendanceStop(Transaction openedTransaction, Attendance current)
		{
			if (openedTransaction == null) {
				throw new ArgumentNullException(nameof(openedTransaction));
			}

			if (current == null) {
				throw new ArgumentNullException(nameof(current));
			}

			//SaveData();

			//// 1. Дистрибьюция
			//for (int c = 2; c < DistributionTable.ChildCount; c = c + 2) {
			//	var row = (LinearLayout)DistributionTable.GetChildAt(c);
			//	var distr = MainDatabase.CreateData<DistributionData>(current.UUID);
			//	distr.DrugSKU = (string)row.GetTag(Resource.String.DrugSKUUUID);
			//	distr.IsExistence = row.FindViewById<CheckBox>(Resource.Id.dtiIsExistenceCB).Checked;
			//	distr.Count = Helper.ToIntExeptNull(row.FindViewById<EditText>(Resource.Id.dtiCountET).Text);
			//	distr.Price = Helper.ToIntExeptNull(row.FindViewById<EditText>(Resource.Id.dtiPriceET).Text);
			//	distr.IsPresence = row.FindViewById<CheckBox>(Resource.Id.dtiIsPresenceCB).Checked;
			//	distr.HasPOS = row.FindViewById<CheckBox>(Resource.Id.dtiHasPOSCB).Checked;
			//	distr.Order = row.FindViewById<EditText>(Resource.Id.dtiOrderET).Text;
			//	distr.Comment = row.FindViewById<EditText>(Resource.Id.dtiCommentET).Text;
			//}

			//// 2. Содержание визита

			//// 2.1 Презентация
			//var presentationTable = View.FindViewById<LinearLayout>(Resource.Id.ipdPresentationTable);
			//for (int c = 0; c < presentationTable.ChildCount; c++) {
			//	var row = presentationTable.GetChildAt(c);
			//	var employeeUUID = (string)row.GetTag(Resource.String.PDEmployee);
			//	if (string.IsNullOrEmpty(employeeUUID)) continue;

			//	var brandTable = row.FindViewById<LinearLayout>(Resource.Id.ipiBrandTable);
			//	for (int cc = 0; cc < brandTable.ChildCount; cc++) {
			//		var subRow = (LinearLayout)brandTable.GetChildAt(cc);
			//		var brandUUID = (string)subRow.GetTag(Resource.String.PDBrand);
			//		if (string.IsNullOrEmpty(brandUUID)) continue;

			//		var workTypes = (string)subRow.FindViewById<TextView>(Resource.Id.ipsiWorkTypesTV).GetTag(Resource.String.PDWorkTypes);
			//		if (string.IsNullOrEmpty(workTypes)) continue;

			//		foreach (var workType in workTypes.Split(';')) {
			//			var presentationData = MainDatabase.CreateData<PresentationData>(current.UUID);
			//			presentationData.Employee = employeeUUID;
			//			presentationData.Brand = brandUUID;
			//			presentationData.WorkType = workType;
			//		}
			//	}
			//}

			//// 2.1 Фарм-кружок
			//var brands = (string)CoterieBrands.GetTag(Resource.String.CDBrands);
			//var employees = (string)CoterieEmployees.GetTag(Resource.String.CDEmployees);
			//if (!string.IsNullOrEmpty(brands) && !string.IsNullOrEmpty(employees)) {
			//	foreach (var brand in brands.Split(';')) {
			//		foreach (var employee in employees.Split(';')) {
			//			var coterieData = MainDatabase.CreateData<CoterieData>(current.UUID);
			//			coterieData.Employee = employee;
			//			coterieData.Brand = brand;
			//		}
			//	}
			//}

			//// 3. Акция
			//var promotionSpinner = PromotionLayout.FindViewById<Spinner>(Resource.Id.ifPromotionS);
			//if (promotionSpinner.SelectedItemPosition > 0) {
			//	var promotionData = MainDatabase.CreateData<PromotionData>(current.UUID);
			//	promotionData.Promotion = Promotions[promotionSpinner.SelectedItemPosition - 1].uuid;
			//	promotionData.Text = PromotionLayout.FindViewById<EditText>(Resource.Id.ifPromotionET).Text;
			//}

			//// 4. Активность конкурентов
			//if (CompetitorLayout.FindViewById<CheckBox>(Resource.Id.ifCompetitorCB).Checked) {
			//	var competitorData = MainDatabase.CreateData<CompetitorData>(current.UUID);
			//	competitorData.Text = CompetitorLayout.FindViewById<EditText>(Resource.Id.ifCompetitorET).Text;
			//}

			//// 5. Сообщения
			//for (int c = 0; c < MessageTable.ChildCount; c++) {
			//	var row = (LinearLayout)MessageTable.GetChildAt(c);
			//	var messageTypeSpinner = row.FindViewById<Spinner>(Resource.Id.imiMessageTypeS);
			//	if (messageTypeSpinner.SelectedItemPosition > 0) {
			//		var messageData = MainDatabase.CreateData<MessageData>(current.UUID);
			//		messageData.Type = MessageTypes[messageTypeSpinner.SelectedItemPosition - 1].uuid;
			//		messageData.Text = row.FindViewById<EditText>(Resource.Id.imiMessageTextET).Text;
			//	}
			//}

			//// 6. Продажи
			//for (int c = 2; c < SaleTable.ChildCount; c = c + 2) {
			//	var row = (LinearLayout)SaleTable.GetChildAt(c);
			//	var skuUUID = (string)row.GetTag(Resource.String.DrugSKUUUID);
			//	for (int m = 0; m < SaleDataMonths.Count(); m++) {
			//		var rView = (TextView)row.GetChildAt(m + 1);
			//		if (string.IsNullOrEmpty(rView.Text)) continue;

			//		var isChanged = (bool)rView.GetTag(Resource.String.IsChanged);
			//		if (isChanged) {
			//			var saleUUID = (string)rView.GetTag(Resource.String.SaleDataUUID);

			//			Console.WriteLine("Info: sku = {0}, value = {1}, sale = {2}", skuUUID, Helper.ToFloat(rView.Text), saleUUID);

			//			if (string.IsNullOrEmpty(saleUUID)) {
			//				var sale = MainDatabase.CreateData<SaleDataByMonth>(current.UUID);
			//				sale.Pharmacy = Pharmacy.UUID;
			//				sale.DrugSKU = skuUUID;
			//				sale.Year = SaleDataMonths[m].Year;
			//				sale.Month = SaleDataMonths[m].Month;
			//				sale.Sale = Helper.ToFloat(rView.Text);
			//			} else {
			//				var sale = MainDatabase.GetEntity<SaleDataByMonth>(saleUUID);
			//				Console.WriteLine("Info: sku = {0}, month = {1}", sale.DrugSKU == skuUUID, sale.Month == SaleDataMonths[m].Month);
			//				sale.Sale = Helper.ToFloat(rView.Text);
			//			}
			//		}
			//	}
			//}

			//// 6.1 Собираем данные в кварталы
			//var calcQuraters = new Stopwatch();
			//calcQuraters.Start();
			//var datas = MainDatabase.GetPharmacyDatas<SaleDataByMonth>(Pharmacy.UUID);
			//var dict = new Dictionary<string, Dictionary<int, List<SaleDataByMonth>[]>>();
			//foreach (var item in datas) {
			//	if (dict.ContainsKey(item.DrugSKU)) {
			//		if (dict[item.DrugSKU].ContainsKey(item.Year)) {
			//			dict[item.DrugSKU][item.Year][(item.Month - 1) / 3].Add(item);
			//		} else {
			//			dict[item.DrugSKU].Add(item.Year, new List<SaleDataByMonth>[4]);
			//			dict[item.DrugSKU][item.Year][0] = new List<SaleDataByMonth>();
			//			dict[item.DrugSKU][item.Year][1] = new List<SaleDataByMonth>();
			//			dict[item.DrugSKU][item.Year][2] = new List<SaleDataByMonth>();
			//			dict[item.DrugSKU][item.Year][3] = new List<SaleDataByMonth>();
			//			dict[item.DrugSKU][item.Year][(item.Month - 1) / 3].Add(item);
			//		}
			//	} else {
			//		dict.Add(item.DrugSKU, new Dictionary<int, List<SaleDataByMonth>[]>());
			//		dict[item.DrugSKU].Add(item.Year, new List<SaleDataByMonth>[4]);
			//		dict[item.DrugSKU][item.Year][0] = new List<SaleDataByMonth>();
			//		dict[item.DrugSKU][item.Year][1] = new List<SaleDataByMonth>();
			//		dict[item.DrugSKU][item.Year][2] = new List<SaleDataByMonth>();
			//		dict[item.DrugSKU][item.Year][3] = new List<SaleDataByMonth>();
			//		dict[item.DrugSKU][item.Year][(item.Month - 1) / 3].Add(item);}
			//}

			//var oldQuarters = MainDatabase.GetPharmacyDatas<SaleDataByQuarter>(Pharmacy.UUID);
			//int newQuarters = 0;
			//foreach (var sku in dict) {
			//	foreach (var year in sku.Value) {
			//		for (int q = 1; q <= 4; q++) {
			//			if (year.Value[q - 1].Count == 3) {
			//				newQuarters++;
			//				var oldQuarter = oldQuarters.SingleOrDefault(oq => oq.DrugSKU == sku.Key && oq.Year == year.Key && oq.Quarter == q);
			//				if (oldQuarter == null) {
			//					var quarter = MainDatabase.Create2<SaleDataByQuarter>();
			//					quarter.Attendance = current.UUID;
			//					quarter.Pharmacy = Pharmacy.UUID;
			//					quarter.DrugSKU = sku.Key;
			//					quarter.Year = year.Key;
			//					quarter.Quarter = q;
			//					quarter.Sale = 0.0f;
			//					foreach (var item in year.Value[q - 1]) {
			//						quarter.Sale += item.Sale;
			//					}
			//					if (quarter.Sale.HasValue) {
			//						if (Math.Abs(quarter.Sale.Value) < 0.01) quarter.Sale = null;
			//					}
			//				} else {
			//					oldQuarter.Sale = 0.0f;
			//					foreach (var item in year.Value[q - 1]) {
			//						oldQuarter.Sale += item.Sale;
			//					}
			//					if (oldQuarter.Sale.HasValue) {
			//						if (Math.Abs(oldQuarter.Sale.Value) < 0.01) oldQuarter.Sale = null;
			//					}
			//				}
			//			}
			//		}
			//	}
			//}
			//calcQuraters.Stop();
			//Console.WriteLine(@"Calc: {0}, Count: {1}, oldQuarters: {2}, newQuarters: {3}", calcQuraters.ElapsedMilliseconds, datas.Count, oldQuarters.Count, newQuarters);


			//// 7. Резюме визита
			//var resumeText = ResumeLayout.FindViewById<EditText>(Resource.Id.ifResumeET);
			//if (string.IsNullOrEmpty(resumeText.Text)) return;
			//var resumeData = MainDatabase.CreateData<ResumeData>(current.UUID);
			//resumeData.Text = resumeText.Text;
		}

		public void OnAttendanceResume()
		{
			throw new NotImplementedException();
		}

		public void OnAttendancePause()
		{
			throw new NotImplementedException();
		}
	}
}

