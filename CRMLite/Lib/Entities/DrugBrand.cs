using Realms;

namespace CRMLite.Entities
{
	public class DrugBrand : RealmObject
	{
		[ObjectId]
		public string uuid { get; set; }

		public string name { get; set; }
	}
}

