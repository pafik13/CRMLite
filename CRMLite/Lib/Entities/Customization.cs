﻿using Realms;

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

		#region Locator customizations
		public const string IsLocatorEnable = "IsLocatorEnable";

		public const string IsLocatorNetRequestOn = "IsLocatorNetRequestOn";
		public const string LocatorNetRequestPeriod = "LocatorNetRequestPeriod";

		public const string IsLocatorGPSRequestOn = "IsLocatorGPSRequestOn";
		public const string LocatorGPSRequestPeriod = "LocatorGPSRequestPeriod";

		public const string LocatorIdlePeriod = "LocatorIdlePeriod";
		#endregion

	}
}
