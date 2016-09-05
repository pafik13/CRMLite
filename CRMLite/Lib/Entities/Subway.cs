using Realms;

namespace CRMLite.Entities
{
	public class Subway : RealmObject, IEntiryFromServer
	{
		[Indexed]
		public string uuid { get; set; }

		public string name { get; set; }

		public string city { get; set; }
	}
}
