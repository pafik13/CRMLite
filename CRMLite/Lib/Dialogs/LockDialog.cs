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
		public const string ARG_BG_COLOR = "ARG_BG_COLOR";


		string Message;
		int BGColor;

		public static LockDialog Create(string message, int bgColor = -1)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException(nameof(message));
			}

			var arguments = new Bundle();
			arguments.PutString(ARG_MESSAGE, message);
			arguments.PutInt(ARG_BG_COLOR, bgColor);

			return new LockDialog() {
				Arguments = arguments
			};
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
			BGColor = Arguments.GetInt(ARG_BG_COLOR);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);
			base.OnCreateView(inflater, container, savedInstanceState);

			TextView view = (TextView)inflater.Inflate(Android.Resource.Layout.SelectDialogItem, container);

			view.Text = Message;
			view.SetTextColor(Android.Graphics.Color.White);
			view.SetBackgroundResource(BGColor == -1 ? Resource.Color.white_bg : BGColor);

			return view;
		}
	}
}

