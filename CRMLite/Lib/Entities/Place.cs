using Realms;

namespace CRMLite.Entities
{
	public class Place : RealmObject
	{
		[Indexed]
		public string uuid { get; set; }

		public string name { get; set; }
	}
}
