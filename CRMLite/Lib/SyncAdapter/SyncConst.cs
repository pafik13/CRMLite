using Android.Net;

namespace CRMLite.Lib.Sync
{
	public static class SyncConst
	{
		public const string AUTHORITY = "ru.sbl.crmlite2.provider";

		public const string ACCOUNT_TYPE = "ru.sbl.crmlite2";

		public const string ACCOUNT = "autosync";

		// Sync interval constants
		public const long SECONDS_PER_MINUTE = 60L;
		public const long SYNC_INTERVAL_IN_MINUTES = 5L;
		public const long SYNC_INTERVAL = SYNC_INTERVAL_IN_MINUTES * SECONDS_PER_MINUTE;

		public const string Attendancies = "Attendancies";
		public const string CompetitorDatas = "CompetitorDatas";
		public const string ContractDatas = "ContractDatas";
		public const string CoterieDatas = "CoterieDatas";
		public const string DistributionDatas = "DistributionDatas";
		public const string DistributorDatas = "DistributorDatas";
		public const string Pharmacies = "Pharmacies";
		public const string Employees = "Employees";
		public const string GPSDatas = "GPSDatas";
		public const string Hospitals = "Hospitals";
		public const string HospitalDatas = "HospitalDatas";
		public const string Messages = "Messages";
		public const string MessageDatas = "MessageDatas";
		public const string PresentationDatas = "PresentationDatas";
		public const string PromotionDatas = "PromotionDatas";
		public const string ResumeDatas = "ResumeDatas";
		public const string RouteItems = "RouteItems";
		public const string ExcludeRouteItems = "ExcludeRouteItems";
		public const string GPSLocations = "GPSLocations";
		public const string PhotoDatas = "PhotoDatas";


		public const string SET_SYNCED = "set_synced";

		public const string _OK = "OK";
		public const string _ERROR = "ERROR";


		public const string Distributor = "Distributor";
		public const string PhotoType = "PhotoType";
		public const string DistributionAgreement = "DistributionAgreement";
		public const string PhotoAgreement = "PhotoAgreement";
		public const string PhotoAfterAttendance = "PhotoAfterAttendance";

		public const string DrugBrand = "DrugBrand";
		public const string DrugSKU = "DrugSKU";
		public const string Pharmacy = "Pharmacy";
		public const string Attendance = "Attendance";
		public const string Net = "Net";
		public const string Category = "Category";
		public const string Customization = "Customization";
		public const string WorkType = "WorkType";
		public const string Employee = "Employee";
		public const string RouteItem = "RouteItem";
		public const string ContractData = "ContractData";
		public const string CoterieData = "CoterieData";
		public const string DistributionData = "DistributionData";
		public const string DistributorData = "DistributorData";
		public const string ExcludeRouteItem = "ExcludeRouteItem";
		public const string FinanceDataByMonth = "FinanceDataByMonth";
		public const string Hospital = "Hospital";
		public const string HospitalData = "HospitalData";
		public const string Message = "Message";
		public const string MessageData = "MessageData";
		public const string PhotoComment = "PhotoComment";
		public const string PhotoData = "PhotoData";
		public const string PresentationData = "PresentationData";
		public const string PromotionData = "PromotionData";
		public const string ResumeData = "ResumeData";
		public const string SaleDataByMonth = "SaleDataByMonth";
		public const string SaleDataByQuarter = "SaleDataByQuarter";

		public static Uri GetURI(string lastPathSegment)
		{
			return Uri.Parse("content://" + AUTHORITY + "/" + lastPathSegment);
		}
	}
}

