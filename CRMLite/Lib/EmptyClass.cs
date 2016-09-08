using Android.Text;
using Java.Util.Regex;

namespace InputFilterForEditText
{
	class DecimalPlaceFilter : Java.Lang.Object, IInputFilter
	{

		int left, right;
		readonly Pattern mPattern;

		public DecimalPlaceFilter(int left, int right)
		{
			this.left = left;
			this.right = right;
			mPattern = Pattern.Compile("^\\d{0," + left + "}([\\.,](\\d{0," + right + "})?)?$");

		}

		public Java.Lang.ICharSequence FilterFormatted(Java.Lang.ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
		{
			string formatedSource = source.SubSequenceFormatted(start, end).ToString();

			string destPrefix = dest.SubSequenceFormatted(0, dstart).ToString();

			string destSuffix = dest.SubSequenceFormatted(dend, dest.Length()).ToString();

			string result = destPrefix + formatedSource + destSuffix;


			Matcher matcher = mPattern.Matcher(result);
			if (!matcher.Matches()) {
				return new Java.Lang.String(string.Empty);
			}
			return null;
		}
	}

}