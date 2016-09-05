using Realms;

namespace CRMLite.Entities
{
	public class Position : RealmObject, IEntiryFromServer
	{
		[Indexed]
		public string uuid { get; set; }

		public string name { get; set; }
	}
}

