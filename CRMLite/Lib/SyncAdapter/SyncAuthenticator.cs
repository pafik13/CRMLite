using System;
using SD = System.Diagnostics;

using Android.Accounts;
using Android.Content;
using Android.OS;

namespace CRMLite.Lib.Sync
{
	public class SyncAuthenticator : AbstractAccountAuthenticator
	{
		Context Context;

		public SyncAuthenticator(Context context): base(context)
		{
			SD.Debug.WriteLine("SyncAuthenticator");
			Context = context;
		}

		public override Bundle AddAccount(AccountAuthenticatorResponse response, string accountType, string authTokenType, string[] requiredFeatures, Bundle options)
		{
			throw new NotImplementedException();
		}

		public override Bundle ConfirmCredentials(AccountAuthenticatorResponse response, Account account, Bundle options)
		{
			throw new NotImplementedException();
		}

		public override Bundle EditProperties(AccountAuthenticatorResponse response, string accountType)
		{
			throw new NotImplementedException();
		}

		public override Bundle GetAuthToken(AccountAuthenticatorResponse response, Account account, string authTokenType, Bundle options)
		{
			throw new NotImplementedException();
		}

		public override string GetAuthTokenLabel(string authTokenType)
		{
			throw new NotImplementedException();
		}

		public override Bundle HasFeatures(AccountAuthenticatorResponse response, Account account, string[] features)
		{
			throw new NotImplementedException();
		}

		public override Bundle UpdateCredentials(AccountAuthenticatorResponse response, Account account, string authTokenType, Bundle options)
		{
			throw new NotImplementedException();
		}
	}
}

