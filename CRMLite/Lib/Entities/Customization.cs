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
		public const string IsPharmacyAddEnable = "IsPharmacyAddEnable";

		#region Locator customizations
		public const string IsLocatorEnable = "IsLocatorEnable";

		public const string IsLocatorNetRequestOn = "IsLocatorNetRequestOn";
		public const string LocatorNetRequestPeriod = "LocatorNetRequestPeriod";

		public const string IsLocatorGPSRequestOn = "IsLocatorGPSRequestOn";
		public const string LocatorGPSRequestPeriod = "LocatorGPSRequestPeriod";

		public const string LocatorIdlePeriod = "LocatorIdlePeriod";
		#endregion

		#region PhotoUploader customizations
		public const string IsPhotoUploaderEnable = "IsPhotoUploaderEnable";
		#endregion
		
		
		#region Reserved UUIDs customizations
		public const string InternshipUUID = "InternshipUUID";
		public const string SickleaveUUID = "SickleaveUUID";
		public const string FullDayTrainingUUID = "FullDayTrainingUUID";
		public const string HalfDayTrainingUUID = "HalfDayTrainingUUID";
		public const string WorkleaveUUID = "WorkleaveUUID";
		#endregion
	}
}
