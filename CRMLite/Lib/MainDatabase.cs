using System;
using System.Linq;
using System.Collections.Concurrent;

using Realms;
using System.IO;

using CRMLite.Entities;
using System.Collections.Generic;

namespace CRMLite
{
	public class MainDatabase
	{
		Realm DB = null;

		ConcurrentDictionary<SyncItem, SyncItem> SyncDictionary = null;

		ConcurrentDictionary<string, SyncResult> ResultDictionary = null;

		//int QueueMaxSize = 20;

		protected static MainDatabase Me;

		static MainDatabase()
		{
			Me = new MainDatabase();
		}

		protected MainDatabase()
		{
			// instantiate the database	
			DB = Realm.GetInstance();

			SyncDictionary = new ConcurrentDictionary<SyncItem, SyncItem>();

			ResultDictionary = new ConcurrentDictionary<string, SyncResult>();
		}

		#region Net
		public static Net GetNet(string uuid)
		{
			return Me.DB.All<Net>().Single(item => item.uuid == uuid);
		}

		public static List<Net> GetNets()
		{
			return Me.DB.All<Net>().ToList();
		}

		public static void SaveNets(List<Net> nets)
		{
			using (var trans = Me.DB.BeginWrite())
			{
				foreach (var item in nets)
				{
					Me.DB.Manage(item);
				}
				trans.Commit();
			}
		}
		#endregion

		#region Position
		public static Position GetPosition(string uuid)
		{
			return Me.DB.All<Position>().Single(item => item.uuid == uuid);
		}

		public static List<Position> GetPositions()
		{
			return Me.DB.All<Position>().ToList();
		}

		public static void SavePositions(List<Position> positions)
		{
			using (var trans = Me.DB.BeginWrite())
			{
				foreach (var item in positions)
				{
					Me.DB.Manage(item);
				}
				trans.Commit();
			}
		}
		#endregion

		#region DrugSKU
		public static DrugSKU GetDrugSKU(string uuid)
		{
			return Me.DB.All<DrugSKU>().Single(item => item.uuid == uuid);
		}

		public static List<DrugSKU> GetDrugSKUs()
		{
			return Me.DB.All<DrugSKU>().ToList();
		}

		public static void SaveDrugSKUs(List<DrugSKU> data)
		{
			using (var trans = Me.DB.BeginWrite())
			{
				foreach (var item in data)
				{
					Me.DB.Manage(item);
				}
				trans.Commit();
			}
		}
		#endregion

		#region DrugBrand
		public static DrugBrand GetDrugBrand(string uuid)
		{
			return Me.DB.All<DrugBrand>().Single(item => item.uuid == uuid);
		}

		public static List<DrugBrand> GetDrugBrands()
		{
			return Me.DB.All<DrugBrand>().ToList();
		}

		public static void SaveDrugBrands(List<DrugBrand> data)
		{
			using (var trans = Me.DB.BeginWrite())
			{
				foreach (var item in data)
				{
					Me.DB.Manage(item);
				}
				trans.Commit();
			}
		}
		#endregion

		#region GENERIC
		public static T GetItem<T>(string uuid) where T : RealmObject, IEntiryFromServer
		{
			return Me.DB.All<T>().Single(item => item.uuid == uuid);
		}

		public static List<T> GetItems<T>() where T : RealmObject
		{
			return Me.DB.All<T>().ToList();
		}

		public static void SaveItems<T>(IList<T> data) where T : RealmObject
		{
			using (var trans = Me.DB.BeginWrite())
			{
				foreach (var item in data)
				{
					Me.DB.Manage(item);
				}
				trans.Commit();
			}
		}
		#endregion

		public static IList<string> GetStates()
		{
			var list = new List<string>();
			list.Add("Активна");
			list.Add("В резерве");
			list.Add("Закрыта");

			return list;
		}

		public static Subway GetSubway(string uuid)
		{
			return Me.DB.All<Subway>().Single(item => item.uuid == uuid);
		}

		public static List<Subway> GetSubways()
		{
			return Me.DB.All<Subway>().ToList();
		}

		public static void SaveSubways(List<Subway> subways)
		{
			using (var trans = Me.DB.BeginWrite())
			{
				foreach (var item in subways)
				{
					Me.DB.Manage(item);
				}
				trans.Commit();
			}
		}

		public static Region GetRegion(string uuid)
		{
			return Me.DB.All<Region>().Single(item => item.uuid == uuid);
		}

		public static List<Region> GetRegions()
		{
			return Me.DB.All<Region>().ToList();
		}

		public static void SaveRegions(List<Region> regions)
		{
			using (var trans = Me.DB.BeginWrite())
			{
				foreach (var item in regions)
				{
					Me.DB.Manage(item);
				}
				trans.Commit();
			}
		}

		public static Category GetCategory(string uuid)
		{
			return Me.DB.All<Category>().Single(item => item.uuid == uuid);
		}

		public static List<Category> GetCategories(string type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if ((type != "net") && (type != "sell"))
			{
				throw new ArgumentException("Illegal value for parameter", nameof(type));
			}

			return Me.DB.All<Category>().Where(item => item.type == type).ToList();
		}

		public static void SaveCategories(List<Category> categories)
		{
			using (var trans = Me.DB.BeginWrite())
			{
				foreach (var item in categories)
				{
					Me.DB.Manage(item);
				}
				trans.Commit();
			}
		}

		public static Place GetPlace(string uuid)
		{
			return Me.DB.All<Place>().Single(item => item.uuid == uuid);
		}

		public static List<Place> GetPlaces()
		{
			return Me.DB.All<Place>().ToList();
		}

		public static void SavePlaces(List<Place> places)
		{
			using (var trans = Me.DB.BeginWrite()) {
				foreach (var item in places) {
					Me.DB.Manage(item);
				}
				trans.Commit();
			}
		}

		#region Sync
		public static void AddToQueue(SyncItem item)
		{
			Me.SyncDictionary[item] = item;
		}

		public static void SaveSyncResult(string objectUUID, SyncResult data)
		{
			Me.ResultDictionary[objectUUID] = data;
		}

		public static SyncResult GetSyncResult(string objectUUID)
		{
			SyncResult result;
			Me.ResultDictionary.TryRemove(objectUUID, out result);
			return result;
		}

		public static ConcurrentDictionary<SyncItem, SyncItem> GetQueue()
		{
			return Me.SyncDictionary;
		}

		public static int GetQueueSize()
		{
			return Me.SyncDictionary.Count();
		}
		#endregion


		public static PresentationData CreatePresentationData(string attendanceUUID)
		{
			var item = Me.DB.CreateObject<PresentationData>();
			item.UUID = Guid.NewGuid().ToString();
			item.Attendance = attendanceUUID;
			return item;
		}

		public static void DeletePresentationDatas(IList<PresentationData> data)
		{
			foreach (var item in data)
			{
				Me.DB.Remove(item);
			}
		}

		public static CoterieData CreateCoterieData(string attendanceUUID)
		{
			var item = Me.DB.CreateObject<CoterieData>();
			item.UUID = Guid.NewGuid().ToString();
			item.Attendance = attendanceUUID;
			return item;
		}

		public static void DeleteCoterieDatas(IList<CoterieData> data)
		{
			foreach (var item in data)
			{
				Me.DB.Remove(item);
			}
		}

		#region Employee
		public static Employee CreateEmployee(string pharmacyUUID)
		{
			var employee = Me.DB.CreateObject<Employee>();
			employee.UUID = Guid.NewGuid().ToString();
			employee.Pharmacy = pharmacyUUID;
			return employee;
		}

		public static void DeleteEmployee(Employee employee)
		{
			Me.DB.Remove(employee);
		}

		public static IList<Employee> GetEmployees(string pharmacyUUID)
		{
			return Me.DB.All<Employee>().Where(item => item.Pharmacy == pharmacyUUID).ToList();
		}
		#endregion

		#region Hospital
		public static Hospital CreateHospital(string pharmacyUUID)
		{
			var hospital = Me.DB.CreateObject<Hospital>();
			hospital.UUID = Guid.NewGuid().ToString();
			hospital.Pharmacy = pharmacyUUID;
			return hospital;
		}

		public static void DeleteHospital(Hospital hospital)
		{
			Me.DB.Remove(hospital);
		}

		public static IList<Hospital> GetHospitals(string pharmacyUUID)
		{
			return Me.DB.All<Hospital>().Where(item => item.Pharmacy == pharmacyUUID).ToList();
		}
		#endregion

		public static void ManageQueue()
		{
			SyncItem item;
			foreach (var sync in Me.SyncDictionary)
			{
				if (sync.Value.IsSynced)
				{
					Me.SyncDictionary.TryRemove(sync.Key, out item);
				}
			}
		}

		public static Transaction BeginTransaction()
		{
			return Me.DB.BeginWrite();
		}

		#region Pharmacy
		public static Pharmacy CreatePharmacy()
		{
			var pharmacy = Me.DB.CreateObject<Pharmacy>();
			pharmacy.UUID = Guid.NewGuid().ToString();
			pharmacy.SetState(PharmacyState.psActive);
			return pharmacy;
		}

		public static void DeletePharmacy(Pharmacy pharmacy)
		{
			Me.DB.Remove(pharmacy);
		}

		public static Pharmacy GetPharmacy(string UUID)
		{
			return Me.DB.All<Pharmacy>().Single(item => item.UUID == UUID);
		}

		public static IList<Pharmacy> GetPharmacies()
		{
			return Me.DB.All<Pharmacy>().ToList();
		}


		public static IList<Pharmacy> GetPharmacies(int count)
		{
			return Me.DB.All<Pharmacy>()
				     .Take(count)
				     .ToList();
		}

		public static IList<Pharmacy> GetPharmacies(int count, int page)
		{
			return Me.DB.All<Pharmacy>()
				     .Skip(page * count)
				     .Take(count)
				     .ToList();
		}

	#endregion
	}
}

