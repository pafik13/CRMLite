using System;

using Realms;

namespace CRMLite
{
	public class MaterialFile: RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }
    
    	public string material { get; set; }
    	    
	    public string fileName { get; set; }

		public string s3Key { get; set; }

		public string s3Location { get; set; }
	}
}

