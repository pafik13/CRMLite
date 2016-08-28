using Realms;

namespace CRMLite.Entities
{
	public class Contract: RealmObject, IEntiryFromServer
	{
		[ObjectId]
		public string uuid { get; set; }

		public string name { get; set; }

		public string description { get; set; }

		[Indexed]
		public string net { get; set; }
	}
}

