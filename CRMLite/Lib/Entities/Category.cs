using Realms;

namespace CRMLite.Entities
{
	public class Category : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }

		/// <summary>
		/// Текущие типы: "net" и "sell"
		/// </summary>
		/// <value>The type.</value>
		public string type { get; set; }
	}
}