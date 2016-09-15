using Realms;

namespace CRMLite.Entities
{
	/// <summary>
	/// Аптечная сеть.
	/// </summary>
	public class Net : RealmObject, IEntiryFromServer
	{
		/// <summary>
		/// Уникальный идентификатор аптечной сети. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string uuid { get; set; }

		/// <summary>
		/// Название аптечной сети.
		/// </summary>
		/// <value>The name.</value>
		public string name { get; set; }

		/// <summary>
		/// Юридическое название аптечной сети.
		/// </summary>
		/// <value>The legal name.</value>
		public string LegalName { get; set; }

		/// <summary>
		/// Описание аптечной сети.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }
	}
}
