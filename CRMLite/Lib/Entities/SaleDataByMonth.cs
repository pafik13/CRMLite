using System;
using Realms;

namespace CRMLite.Entities
{
	/// <summary>
	/// Данные по продажам, которые вводятся на визите
	/// </summary>
	public class SaleDataByMonth : RealmObject, IAttendanceData, IPharmacyData, IEntity, ISync
	{
		/// <summary>
		/// Уникальный идентификатор продаж. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		[Indexed]
		public string Attendance { get; set; }

		[Indexed]
		public string Pharmacy { get; set; }

		public string DrugSKU { get; set; }

		public int Year { get; set; }

		public int Month { get; set; }

		public float? Sale { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
	}
}

