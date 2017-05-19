using System;
using Realms;

namespace CRMLite.Entities
{
	public class DistributionData : RealmObject, IAttendanceData, IEntity, ISync
	{
		/// <summary>
		/// Уникальный идентификатор информации о дистрибуции. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		public string DrugSKU { get; set; }

		public bool IsExistence { get; set; }

		public int Count { get; set; }

		public int Price { get; set; }

		public bool IsPresence { get; set; }

		public bool HasPOS { get; set; }

		public string Order { get; set; }

		public string Comment { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public SyncResult SyncResult { get; set; }

		public bool IsSynced { get; set; }

		public bool isDummyBool { get; set; }
	}
}

