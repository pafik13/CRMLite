using Realms;

namespace CRMLite.Entities
{
	/// <summary>
	/// Данные по продажам, которые вводятся на визите
	/// </summary>
	public class SaleDataByMonth : RealmObject, IAttendanceData, IPharmacyData, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор продаж. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		[Indexed]
		public string Attendance { get; set; }

		[Indexed]
		public string Pharmacy { get; set; }

		public string DrugSKU { get; set; }

		public int Year { get; set; }

		public int Month { get; set; }

		public float? Sale { get; set; }
	}
}

