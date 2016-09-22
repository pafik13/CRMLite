using System;
using Realms;

namespace CRMLite.Entities
{
	public class RouteItem : RealmObject, IPharmacyData, IEntity, ISync
	{
		[PrimaryKey]
		public string UUID { get; set; }

		[Indexed]
		public string Pharmacy { get; set; }

		public DateTimeOffset Date { get; set; }

		public int Order { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public SyncResult SyncResult { get; set; }

		public bool IsSynced { get; set; }
	}
}

