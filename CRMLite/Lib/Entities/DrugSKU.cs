using Realms;

namespace CRMLite.Entities
{
	public class DrugSKU : RealmObject, IEntiryFromServer
	{
		[ObjectId]
		public string uuid { get; set; }

		public string name { get; set; }

		[Indexed]
		public string brand { get; set; }
	}
}

