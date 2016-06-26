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

		int QueueMaxSize = 20;

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

