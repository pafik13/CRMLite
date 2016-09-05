using Realms;

namespace CRMLite.Entities
{
	public class PresentationData : RealmObject, IEntity, IAttendanceData
	{
		/// <summary>
		/// Уникальный идентификатор презентации. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		[Indexed]
		public string Attendance { get; set; }

		public string Employee { get; set; }

		public string Brand { get; set; }

		public string WorkType { get; set; }
	}

	public class PresentationDataKey
	{
		public Employee Employee { get; set; }

		public DrugBrand Brand { get; set; }		
	}
}

