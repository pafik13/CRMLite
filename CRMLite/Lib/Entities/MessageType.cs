using Realms;

namespace CRMLite.Entities
{
	public class MessageType : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }
	}
}

