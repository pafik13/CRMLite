using Realms;

namespace CRMLite
{
	public class PhotoType : RealmObject, IEntiryFromServer
	{
		[ObjectId]
		public string uuid { get; set; }

		public string name { get; set; }

		public bool isNeedBrand { get; set; }
	}
}

