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

		public static string HockeyappAppId {
			get { return "c5a5b39231634cbbbd2068c7a1bd6d1d"; }
		}
		
		public static string AWSSecretKey {
			get { return "AJ7VnfTi+srwQhIm0200qYOcZF3v2tiQuQQnfVLz"; }
		}

		public static string AWSAccessKeyId {
			get { return "AKIAI3HKNL4B2W5BFKJA"; }
		}


		public static bool IsNeedReCreateDB
		{
			get { return true; }
		}
	}
}

