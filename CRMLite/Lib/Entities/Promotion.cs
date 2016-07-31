using Realms;

namespace CRMLite
{
	public class Promotion : RealmObject, IEntiryFromServer
	{
		[ObjectId]
		public string uuid { get; set; }

		public string name { get; set; }
	}
}

