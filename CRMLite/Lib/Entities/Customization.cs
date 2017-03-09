using Realms;

namespace CRMLite.Entities
{
	public class Customization : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }
		
		public string key { get; set; }
		
		public string type { get; set; }

		public string value { get; set; }
	}
	
	public static class Customizations
	{
		public const string AttendanceMinPeriod = "AttendanceMinPeriod";
		public const string IsLocatorEnable = "IsLocatorEnable";
	}
}
