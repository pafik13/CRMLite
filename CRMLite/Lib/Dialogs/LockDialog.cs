using System;

//using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using Android.Support.V4.App;

namespace CRMLite.Dialogs
{
	public class LockDialog : DialogFragment
	{
		public const string TAG = "LockDialog";
		public const string ARG_MESSAGE = "ARG_MESSAGE";

		string Message;

		public static LockDialog Create(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException(nameof(message));
			}

			var lockDialog = new LockDialog();
			var arguments = new Bundle();
			arguments.PutString(ARG_MESSAGE, message);
			lockDialog.Arguments = arguments;
			return lockDialog;
		}

		public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			var dialog = base.OnCreateDialog(savedInstanceState);

			// request a window without the title
			dialog.Window.RequestFeature(WindowFeatures.NoTitle);
			return dialog;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
			Message = Arguments.GetString(ARG_MESSAGE);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);
			base.OnCreateView(inflater, container, savedInstanceState);

			View view = inflater.Inflate(Android.Resource.Layout.SelectDialogItem, container);

			if (view is TextView)
			{
				((TextView)view).Text = Message;
				((TextView)view).SetTextColor(Android.Graphics.Color.White);
				((TextView)view).SetBackgroundColor(Android.Graphics.Color.Blue);
			}

			return view;
		}
	}
}

