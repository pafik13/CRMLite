using Realms;

namespace CRMLite.Entities
{
	public class PromotionData : RealmObject, IAttendanceData, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор акции. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		public string Promotion { get; set; }

		public string Text { get; set; }
	}
}
