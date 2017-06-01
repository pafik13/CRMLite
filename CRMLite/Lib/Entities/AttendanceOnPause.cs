using Realms;

namespace CRMLite.Entities
{
	// PausedAtts
	[Preserve(AllMembers = true)]
	public class AttendanceOnPause : RealmObject, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор посещения. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }
	}
}

