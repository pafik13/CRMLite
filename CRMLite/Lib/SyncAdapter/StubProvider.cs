using System;
using System.Linq;
using System.Collections.Generic;


using Android.Content;
using CRMLite.Entities;
using Realms;

namespace CRMLite.Lib.Sync
{
	public class StubProvider : ContentProvider
	{
		public const string AUTHORITY = "ru.sbl.crmlite2.provider";


		NewtonsoftJsonSerializer Serializer;

		public override bool OnCreate()
		{
			Serializer = new NewtonsoftJsonSerializer();
			return true;
		}

		public override string GetType(Android.Net.Uri uri)
		{
			if (uri.LastPathSegment.Equals("attendancies")) {
				return MainDatabase.DBPath;
			}

			return string.Empty;
		}

		public static IQueryable<IEntity> GetItemsToSync<T>(Realm db) where T : RealmObject, IEntity, ISync
		{
			if (db == null) {
				throw new ArgumentNullException(nameof(db));
			}

			return db.All<T>().Where(item => !item.IsSynced);
		}

		public override Android.Database.ICursor Query(Android.Net.Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
		{
			using (var DB = Realm.GetInstance(MainDatabase.DBPath)) {
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
					default:
						break ;
				}

				if (entities == null) return cursor;

				foreach (var attendance in entities) {
					var values = new Java.Lang.Object[] { type, attendance.UUID, Serializer.Serialize(attendance) };
					cursor.AddRow(values);
				}

				return cursor;
			}
		}

		public override Android.Net.Uri Insert(Android.Net.Uri uri, ContentValues values)
		{
			return null;
		}

		public override int Delete(Android.Net.Uri uri, string selection, string[] selectionArgs)
		{
			return 0;
		}

		public override int Update(Android.Net.Uri uri, ContentValues values, string selection, string[] selectionArgs)
		{
			using (var DB = Realm.GetInstance(MainDatabase.DBPath)) {
				switch (uri.LastPathSegment) {
					case SyncConst.SET_SYNCED:
						if (selectionArgs.Length > 0) {
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
								default:
									break;
									
							}
							using (var transaction = DB.BeginWrite()) {
								var items = entities.Where(e => selectionArgs.Contains(e.UUID));
								foreach (var item in items) {
									//var item = entities.Single(att => att.UUID == uuid);
									if (item is ISync) {
										((ISync)item).IsSynced = true;
									}
								}

								transaction.Commit();
							}
						}
						break;
					default:
						break;
				}
			}
			return selectionArgs.Length;
		}
	}
}

