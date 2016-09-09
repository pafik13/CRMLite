using Realms;

namespace CRMLite
{
	/// <summary>
	/// Квартальные данные по продажам, которые вводятся на визите
	/// </summary>
	public class SaleDataByQuarter: RealmObject, IAttendanceData, IPharmacyData, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор квартальных данных по продажам. Используется Guid.
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

		public int Quarter { get; set; }

		public float? Sale { get; set; }
	}
}

