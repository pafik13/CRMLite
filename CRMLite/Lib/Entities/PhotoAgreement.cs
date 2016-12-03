using Realms;

namespace CRMLite.Entities
{
	public class PhotoAgreement : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string object_type { get; set; }

		public string object_uuid { get; set; }

		public string photoType { get; set; }

		public string brand { get; set; }

		public string comment { get; set; }
	}
}

