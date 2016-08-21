using Realms;

namespace CRMLite.Entities
{
	public class MessageData : RealmObject, IAttendanceData
	{
		/// <summary>
		/// Уникальный идентификатор сообщения от аптеки. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		public string Type { get; set; }

		public string Text { get; set; }

	}
}
