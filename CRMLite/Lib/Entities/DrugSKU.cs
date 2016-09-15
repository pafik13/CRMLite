using Realms;

namespace CRMLite.Entities
{
	public class DrugSKU : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }

		[Indexed]
		public string brand { get; set; }
	}
}

