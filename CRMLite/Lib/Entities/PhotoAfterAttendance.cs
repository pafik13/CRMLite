using Realms;

namespace CRMLite.Entities
{
	public class PhotoAfterAttendance : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string photoType { get; set; }
	}
}

