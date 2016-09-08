using Realms;

namespace CRMLite.Entities
{
	public class DistributionData : RealmObject, IAttendanceData, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор информации о дистрибуции. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
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
	}
}

