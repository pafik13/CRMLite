namespace CRMLite
{
	public static class Secret
	{
		public static string DadataApiToken
		{
			get { return "20fa30fc9424a1021131c67104703544a9d6d859"; }
		}

		public static string DadataApiURL {
			get { return "https://suggestions.dadata.ru/suggestions/api/4_1/rs"; }
		}

		public static bool IsNeedReCreateDB
		{
			get { return false; }
		}
	}
}

