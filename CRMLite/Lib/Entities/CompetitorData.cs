using Realms;

namespace CRMLite.Entities
{
	public class CompetitorData : RealmObject, IAttendanceData, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор активности конкурентов. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		public string Text { get; set; }
	}
}

