using Realms;

namespace CRMLite
{
	public class Promotion : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }
	}
}

