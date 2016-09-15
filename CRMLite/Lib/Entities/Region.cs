using Realms;

namespace CRMLite.Entities
{
	public class Region : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }

		public string city { get; set; }
	}
}
