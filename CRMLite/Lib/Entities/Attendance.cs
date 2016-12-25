using System;
using Realms;

namespace CRMLite.Entities
{
 	// Attendancies
	public class Attendance : RealmObject, IEntity, ISync
	{
		/// <summary>
		/// Уникальный идентификатор посещения. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public string Pharmacy { get; set; }

		public DateTimeOffset When { get; set; }

		public double Duration { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
	}
}

