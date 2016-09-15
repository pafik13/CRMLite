using Realms;

namespace CRMLite
{
	public class Material: RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }
	}
}

