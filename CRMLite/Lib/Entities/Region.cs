using Realms;

namespace CRMLite.Entities
{
	public class Region : RealmObject
	{
		[Indexed]
		public string uuid { get; set; }

		public string name { get; set; }

		public string city { get; set; }
	}
}
