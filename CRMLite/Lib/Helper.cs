using System;
using System.Globalization;

namespace CRMLite
{
	public static class Helper
	{
		public static float? ToFloat(string value, int divider = 1)
		{
			if (string.IsNullOrEmpty(value)) return null;

			float result;
			float.TryParse(value.Replace(',', '.'), NumberStyles.Float, new CultureInfo("en-US").NumberFormat, out result);

			if (float.IsInfinity(result) || Math.Abs(result) < 0.01f) return null;

			result /= divider;

			if (float.IsInfinity(result) || Math.Abs(result) < 0.01f) return null;

			return result;
		}

		public static float ToFloatExeptNull(string value)
		{
			if (string.IsNullOrEmpty(value)) return 0.0f;

			float result;
			float.TryParse(value.Replace(',', '.'), NumberStyles.Float, new CultureInfo("en-US").NumberFormat, out result);

			if (float.IsInfinity(result) || Math.Abs(result) < 0.01f) return 0.0f; ;

			return result;
		}
	}
}

