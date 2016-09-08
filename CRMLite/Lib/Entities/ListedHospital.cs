using Realms;

namespace CRMLite.Entities
{
	public class ListedHospital: RealmObject, IEntiryFromServer
	{
		[ObjectId]
		public string uuid { get; set; }

		public string name { get; set; }

		public string address { get; set; }

		public float? latitude { get; set;}

		public float? longitude { get; set;}
	}
}

