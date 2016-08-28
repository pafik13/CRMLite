using System;
using Realms;

namespace CRMLite.Entities
{
	/// <summary>
	/// Данные по продажам, которые вводятся на визите
	/// </summary>
	public class SaleData : RealmObject, IAttendanceData, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор продаж. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		//public string Pharmacy { get; set; }

		public string DrugSKU { get; set; }

		public DateTimeOffset Month { get; set; }

		public float? Sale { get; set; }
	}
}
