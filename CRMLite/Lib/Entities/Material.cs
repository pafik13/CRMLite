using Realms;

namespace CRMLite
{
	public class Material: RealmObject, IEntiryFromServer
	{
		[ObjectId]
		public string uuid { get; set; }

		public string name { get; set; }
	}
}

