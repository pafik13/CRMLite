using Realms;

namespace RealmAndroid.Entities
{
	/// <summary>
	/// Собираемая информация (например: наличие, цена и т.д.).
	/// </summary>
	public class Info : RealmObject
	{
		/// <summary>
		/// Уникальный идентификатор собираемой ифнормации. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		public string UUID { get; set; }

		/// <summary>
		/// Название собираемой ифнормации.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Тип собираемой ифнормации  (например: boolean, integer, float или list)
		/// </summary>
		/// <value>The type of the value.</value>
		public string ValueType { get; set; }
	}
}

