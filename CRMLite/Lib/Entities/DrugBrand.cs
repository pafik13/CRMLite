using Realms;

namespace CRMLite.Entities
{
	public class DrugBrand : RealmObject, IEntiryFromServer
	{
		[ObjectId]
		public string uuid { get; set; }

		public string name { get; set; }
	}
}

