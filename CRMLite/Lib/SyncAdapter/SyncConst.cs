using Android.Net;
namespace CRMLite.Lib.Sync
{
	public static class SyncConst
	{
		public const string AUTHORITY = "ru.sbl.crmlite2.provider";

		public const string ACCOUNT_TYPE = "ru.sbl.crmlite2";

		public const string ACCOUNT = "dummyaccount";

		public const string Attendancies = "Attendancies";
		public const string CompetitorDatas = "CompetitorDatas";
		public const string ContractDatas = "ContractDatas";
		public const string CoterieDatas = "CoterieDatas";
		public const string DistributionDatas = "DistributionDatas";
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


		public const string SET_SYNCED = "set_synced";


		public static Uri GetURI(string lastPathSegment) { 
			return Uri.Parse("content://" + AUTHORITY + "/" + lastPathSegment); 
		}
	}
}

