using System.Linq;
using System.Collections.Generic;

using Android.Net;
using Android.Util;
using Android.Content;

using Newtonsoft.Json;

using Realms;

using CRMLite.Entities;

namespace CRMLite.Lib.Sync
{
	public class StubProvider : ContentProvider
	{
		public const string AUTHORITY = "ru.sbl.crmlite2.provider";

		const string TAG = "CRMLite.Lib.Sync:StubProvider";

		NewtonsoftJsonSerializer Serializer;


		public override bool OnCreate()
		{
			Serializer = new NewtonsoftJsonSerializer();
			return true;
		}

		public override string GetType(Uri uri)
		{
			if (uri.LastPathSegment.Equals("attendancies")) {
				return MainDatabase.DBPath;
			}

			return string.Empty;
		}

		public static IQueryable<IEntity> GetItemsToSync<T>(Realm db) where T : RealmObject, IEntity, ISync
		{
			if (db == null) {
				throw new System.ArgumentNullException(nameof(db));
			}

			return db.All<T>().Where(item => !item.IsSynced);
		}

		public override Android.Database.ICursor Query(Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
		{
			string db_path = selectionArgs[0];
			using (var DB = Realm.GetInstance(db_path)) {
				var fields = new string[] { "TYPE", "UUID", "JSON" };
				var cursor = new Android.Database.MatrixCursor(fields);

				string type = string.Empty;
				IQueryable<IEntity> entities = null;
				switch (uri.LastPathSegment) {
					case SyncConst.Attendancies:
						type = typeof(Attendance).Name;
						entities = GetItemsToSync<Attendance>(DB);
						break;
					case SyncConst.CompetitorDatas:
						type = typeof(CompetitorData).Name;
						entities = GetItemsToSync<CompetitorData>(DB);
						break;
					case SyncConst.ContractDatas:
						type = typeof(ContractData).Name;
						entities = GetItemsToSync<ContractData>(DB);
						break;
					case SyncConst.CoterieDatas:
						type = typeof(CoterieData).Name;
						entities = GetItemsToSync<CoterieData>(DB);
						break;
					case SyncConst.DistributionDatas:
						type = typeof(DistributionData).Name;
						entities = GetItemsToSync<DistributionData>(DB);
						break;
					case SyncConst.DistributorDatas:
						type = typeof(DistributorData).Name;
						entities = GetItemsToSync<DistributorData>(DB);
						break;
					case SyncConst.Pharmacies:
						type = typeof(Pharmacy).Name;
						entities = GetItemsToSync<Pharmacy>(DB);
						break;
					case SyncConst.Employees:
						type = typeof(Employee).Name;
						entities = GetItemsToSync<Employee>(DB);
						break;
					case SyncConst.GPSDatas:
						type = typeof(GPSData).Name;
						entities = GetItemsToSync<GPSData>(DB);
						break;
					case SyncConst.Hospitals:
						type = typeof(Hospital).Name;
						entities = GetItemsToSync<Hospital>(DB);
						break;
					case SyncConst.HospitalDatas:
						type = typeof(HospitalData).Name;
						entities = GetItemsToSync<HospitalData>(DB);
						break;
					case SyncConst.Messages:
						type = typeof(Message).Name;
						entities = GetItemsToSync<Message>(DB);
						break;
					case SyncConst.MessageDatas:
						type = typeof(MessageData).Name;
						entities = GetItemsToSync<MessageData>(DB);
						break;
					case SyncConst.PresentationDatas:
						type = typeof(PresentationData).Name;
						entities = GetItemsToSync<PresentationData>(DB);
						break;
					case SyncConst.PromotionDatas:
						type = typeof(PromotionData).Name;
						entities = GetItemsToSync<PromotionData>(DB);
						break;
					case SyncConst.ResumeDatas:
						type = typeof(ResumeData).Name;
						entities = GetItemsToSync<ResumeData>(DB);
						break;
					case SyncConst.RouteItems:
						type = typeof(RouteItem).Name;
						entities = GetItemsToSync<RouteItem>(DB);
						break;
					case SyncConst.ExcludeRouteItems:
						type = typeof(ExcludeRouteItem).Name;
						entities = GetItemsToSync<ExcludeRouteItem>(DB);
						break;
					case SyncConst.GPSLocations:
						type = typeof(GPSLocation).Name;
						entities = GetItemsToSync<GPSLocation>(DB);
						break;
					default:
						Log.Error(TAG, "Unhandled LastPathSegment:{0}; In {1}", uri.LastPathSegment, "StubProvider.Query");
						break;
				}

				if (entities == null) return cursor;

				foreach (var entity in entities) {
					var values = new Java.Lang.Object[] { type, entity.UUID, Serializer.Serialize(entity) };
					cursor.AddRow(values);
				}

				return cursor;
			}
		}

		Uri ManageItem<T>(Uri uri, Realm db, T item, IQueryable<T> list, string uuid) where T: RealmObject
		{
			var ERROR = new Uri.Builder()
			                   .Scheme(uri.Scheme)
			                   .Authority(uri.Authority)
			                   .Path(SyncConst._ERROR)
			                   .Build();

			var OK = new Uri.Builder()
							.Scheme(uri.Scheme)
							.Authority(uri.Authority)
							.Path(SyncConst._OK)
							.Build();
			
			if (item == null) {
				Log.Error(TAG, "Cannot insert object:{0}:{1}. item is NULL.", uri.LastPathSegment, uuid);
				return ERROR;
			}

			if (list.Count() > 1) {
				Log.Error(TAG, "Cannot insert object:{0}:{1}. Find more than 1 record.", uri.LastPathSegment, uuid);
				return ERROR;
			}

			try {
				using (var transaction = db.BeginWrite()) {
					if (list.Count() == 1) {
						db.Remove(list.First());
					}
					db.Manage(item);
					transaction.Commit();
				}
				return OK;
			} catch (System.Exception ex) {
				Log.Error(TAG, "Cannot insert object:{0}:{1}. Exeption: {2}", uri.LastPathSegment, uuid, ex.Message);
				return ERROR;
			}
		}

		public override Uri Insert(Uri uri, ContentValues values)
		{
			var json = values.GetAsString("json");
			var db_path = values.GetAsString("db_path");
			using (var DB = Realm.GetInstance(db_path)) {
				switch (uri.LastPathSegment) {
					case SyncConst.Distributor: {
							var item = JsonConvert.DeserializeObject<Distributor>(json);
							var list = DB.All<Distributor>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					case SyncConst.PhotoType: {
							var item = JsonConvert.DeserializeObject<PhotoType>(json);
							var list = DB.All<PhotoType>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					case SyncConst.DistributionAgreement: {
							var item = JsonConvert.DeserializeObject<DistributionAgreement>(json);
							var list = DB.All<DistributionAgreement>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					case SyncConst.PhotoAgreement: {
							var item = JsonConvert.DeserializeObject<PhotoAgreement>(json);
							var list = DB.All<PhotoAgreement>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					case SyncConst.PhotoAfterAttendance: {
							var item = JsonConvert.DeserializeObject<PhotoAfterAttendance>(json);
							var list = DB.All<PhotoAfterAttendance>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					case SyncConst.DrugSKU: {
							var item = JsonConvert.DeserializeObject<DrugSKU>(json);
							var list = DB.All<DrugSKU>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					case SyncConst.DrugBrand: {
							var item = JsonConvert.DeserializeObject<DrugBrand>(json);
							var list = DB.All<DrugBrand>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					case SyncConst.Net: {
							var item = JsonConvert.DeserializeObject<Net>(json);
							var list = DB.All<Net>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					case SyncConst.Category: {
							var item = JsonConvert.DeserializeObject<Category>(json);
							var list = DB.All<Category>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					case SyncConst.Customization: {
							var item = JsonConvert.DeserializeObject<Customization>(json);
							var list = DB.All<Customization>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					case SyncConst.WorkType: {
							var item = JsonConvert.DeserializeObject<WorkType>(json);
							var list = DB.All<WorkType>().Where(d => d.uuid == item.uuid);
							return ManageItem(uri, DB, item, list, item.uuid);
						}
					default:
						return new Uri.Builder()
							          .Scheme(uri.Scheme)
							          .Authority(uri.Authority)
							          .Path(SyncConst._ERROR)
							          .Build();
				}

			}

		}

		int RemoveItems<T>(Realm db, IQueryable<T> list, string uuid) where T : RealmObject
		{
			if (list.Count() == 0) return 0;

			if (list.Count() > 1) {
				Log.Error(TAG, "Cannot remove object:{0}:{1}. Find more than 1 record.", typeof(T).Name, uuid);
				return -1;
			}

			try {
				using (var transaction = db.BeginWrite()) {
					db.Remove(list.First());
					transaction.Commit();
				}
				return 1;
			} catch (System.Exception ex) {
				Log.Error(TAG, "Cannot remove object:{0}:{1}. Exeption: {2}", typeof(T).Name, uuid, ex.Message);
				return -1;
			}
		}

		public override int Delete(Uri uri, string selection, string[] selectionArgs)
		{
			string uuid = selectionArgs[1];
			string db_path = selectionArgs[0];
			using (var DB = Realm.GetInstance(db_path)) {
				switch (selection) {
					case SyncConst.Distributor: {
							var list = DB.All<Distributor>().Where(item => item.uuid == uuid);
							return RemoveItems(DB, list, uuid);
						}
					case SyncConst.PhotoType: {
							var list = DB.All<PhotoType>().Where(item => item.uuid == uuid);
							return RemoveItems(DB, list, uuid);
						}
					case SyncConst.DistributionAgreement: {
							var list = DB.All<DistributionAgreement>().Where(item => item.uuid == uuid);
							return RemoveItems(DB, list, uuid);
						}
					case SyncConst.PhotoAgreement: {
							var list = DB.All<PhotoAgreement>().Where(item => item.uuid == uuid);
							return RemoveItems(DB, list, uuid);
						}
					case SyncConst.PhotoAfterAttendance: {
							var list = DB.All<PhotoAfterAttendance>().Where(item => item.uuid == uuid);
							return RemoveItems(DB, list, uuid);
						}
					case SyncConst.DrugSKU: {
							var list = DB.All<DrugSKU>().Where(item => item.uuid == uuid);
							return RemoveItems(DB, list, uuid);
						}
					case SyncConst.DrugBrand: {
							var list = DB.All<DrugBrand>().Where(item => item.uuid == uuid);
							return RemoveItems(DB, list, uuid);
						}
					case SyncConst.Pharmacy: {
							var list = DB.All<Pharmacy>().Where(item => item.UUID == uuid);
							return RemoveItems(DB, list, uuid);
						}
					case SyncConst.Attendance: {
							var list = DB.All<Attendance>().Where(item => item.UUID == uuid);
							return RemoveItems(DB, list, uuid);
						}
					default:
						Log.Error(TAG, "Unhandled selection:{0}; In {1}", selection, "StubProvider.Delete");
						return -1;
				}
			}
		}


		public override int Update(Uri uri, ContentValues values, string selection, string[] selectionArgs)
		{
			string db_path = selectionArgs[0];
			using (var DB = Realm.GetInstance(db_path)) {
				switch (uri.LastPathSegment) {
					case SyncConst.SET_SYNCED:
						if (selectionArgs.Length > 1) {
							var entities = new List<IEntity>();
							switch (selection) {
								case SyncConst.Attendancies:
									entities = DB.All<Attendance>().ToList<IEntity>();
									break;
								case SyncConst.CompetitorDatas:
									entities = DB.All<CompetitorData>().ToList<IEntity>();
									break;
								case SyncConst.ContractDatas:
									entities = DB.All<ContractData>().ToList<IEntity>();
									break;
								case SyncConst.CoterieDatas:
									entities = DB.All<CoterieData>().ToList<IEntity>();
									break;
								case SyncConst.DistributionDatas:
									entities = DB.All<DistributionData>().ToList<IEntity>();
									break;
								case SyncConst.DistributorDatas:
									entities = DB.All<DistributorData>().ToList<IEntity>();
									break;
								case SyncConst.Pharmacies:
									entities = DB.All<Pharmacy>().ToList<IEntity>();
									break;
								case SyncConst.Employees:
									entities = DB.All<Employee>().ToList<IEntity>();
									break;
								case SyncConst.GPSDatas:
									entities = DB.All<GPSData>().ToList<IEntity>();
									break;
								case SyncConst.Hospitals:
									entities = DB.All<Hospital>().ToList<IEntity>();
									break;
								case SyncConst.HospitalDatas:
									entities = DB.All<HospitalData>().ToList<IEntity>();
									break;
								case SyncConst.Messages:
									entities = DB.All<Message>().ToList<IEntity>();
									break;
								case SyncConst.MessageDatas:
									entities = DB.All<MessageData>().ToList<IEntity>();
									break;
								case SyncConst.PresentationDatas:
									entities = DB.All<PresentationData>().ToList<IEntity>();
									break;
								case SyncConst.PromotionDatas:
									entities = DB.All<PromotionData>().ToList<IEntity>();
									break;
								case SyncConst.ResumeDatas:
									entities = DB.All<ResumeData>().ToList<IEntity>();
									break;
								case SyncConst.RouteItems:
									entities = DB.All<RouteItem>().ToList<IEntity>();
									break;
								case SyncConst.ExcludeRouteItems:
									entities = DB.All<ExcludeRouteItem>().ToList<IEntity>();
									break;
								case SyncConst.GPSLocations:
									entities = DB.All<GPSLocation>().ToList<IEntity>();
									break;
								default:
									Log.Error(TAG, "Unhandled selection:{0}; In {1}", selection, "StubProvider.Update");
									break;
									
							}
							using (var transaction = DB.BeginWrite()) {
								var items = entities.Where(e => selectionArgs.Contains(e.UUID));
								var isGPS = selection == SyncConst.GPSDatas || selection == SyncConst.GPSLocations;
								var isSync = (entities.Count > 0) && (entities[0] is ISync);
								if (isGPS) {
									foreach (var item in items) {
										DB.Remove(item as RealmObject);
									}
								} else if (isSync) {
									foreach (var item in items) {
										(item as ISync).IsSynced = true;
									}
								} else {
									Log.Error(TAG, "UnUpdated/UnDeleted selection:{0}; In {1}", selection, "StubProvider.Update");
								}

								transaction.Commit();
							}
						}
						break;
					default:
						Log.Error(TAG, "Unhandled LastPathSegment:{0}; In {1}", uri.LastPathSegment, "StubProvider.Update");
						break;
				}
			}
			return selectionArgs.Length - 1;
		}
	}
}

