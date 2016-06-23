using Realms;

namespace RealmAndroid.Entities
{
	/// <summary>
	/// Вид работы (например: листовка, презентация и т.д.).
	/// </summary>
	public class WorkType : RealmObject
	{
		/// <summary>
		/// Уникальный идентификатор вида работы. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		public string UUID { get; set; }

		/// <summary>
		/// Название вида работы.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }
	}
}

