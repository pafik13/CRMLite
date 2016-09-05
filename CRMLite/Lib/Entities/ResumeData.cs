using Realms;

namespace CRMLite.Entities
{
	public class ResumeData : RealmObject, IAttendanceData, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор резюме визита. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		public string Text { get; set; }
	}
}

