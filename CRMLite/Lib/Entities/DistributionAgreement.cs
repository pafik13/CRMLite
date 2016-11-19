using Realms;

namespace CRMLite.Entities
{
	public class DistributionAgreement : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

 		public string drugSKU { get; set; }

		public string object_type { get; set; }
    
    	public string object_uuid { get; set; }
    
	  	public bool isExistence { get; set; }

		public int count { get; set; }

		public int price { get; set; }

		public bool isPresence { get; set; }

		public bool hasPOS { get; set; }

		public string order { get; set; }

		public string comment { get; set; }
	}
}

