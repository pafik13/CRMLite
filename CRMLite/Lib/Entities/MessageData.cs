using Realms;

namespace CRMLite.Entities
{
	public class MessageData : RealmObject, IAttendanceData, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор сообщения от аптеки. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		public string Type { get; set; }

		public string Text { get; set; }

	}
}
