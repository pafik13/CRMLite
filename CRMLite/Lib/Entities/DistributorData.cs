using System;
using Realms;

namespace CRMLite.Entities
{
	public class DistributorData : RealmObject, IEntity, IAttendanceData, ISync
	{
		/// <summary>
		/// Уникальный идентификатор данных о дистрибьютере. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		[Indexed]
		public string Attendance { get; set; }

		public string Distributor { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
	}
}

