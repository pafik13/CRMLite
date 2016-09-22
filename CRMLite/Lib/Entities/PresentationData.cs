using System;
using Realms;

namespace CRMLite.Entities
{
	public class PresentationData : RealmObject, IEntity, IAttendanceData, ISync
	{
		/// <summary>
		/// Уникальный идентификатор презентации. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		[Indexed]
		public string Attendance { get; set; }

		public string Employee { get; set; }

		public string Brand { get; set; }

		public string WorkType { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public SyncResult SyncResult { get; set; }

		public bool IsSynced { get; set; }
	}

	public class PresentationDataKey
	{
		public Employee Employee { get; set; }

		public DrugBrand Brand { get; set; }		
	}
}

