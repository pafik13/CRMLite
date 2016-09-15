
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite
{
	public class MessageDialog : DialogFragment
	{
		public const string TAG = @"MessageDialog";

		public event EventHandler AfterSaved;

		protected virtual void OnAfterSaved(EventArgs e)
		{
			if (AfterSaved != null) {
				AfterSaved(this, e);
			}
		}

		EditText Text;

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			Dialog.SetCanceledOnTouchOutside(true);
			Dialog.SetTitle(@"Сообщение менеджеру/разработчику");
			//Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

			View view = inflater.Inflate(Resource.Layout.MessageDialog, container, false);

			var close = view.FindViewById<Button>(Resource.Id.Close);
			close.Click += (sender, e) => {
				Dismiss();
			};

			var save = view.FindViewById<Button>(Resource.Id.Save);
			save.Click += Save_Click;

			Text = view.FindViewById<EditText>(Resource.Id.mdTextET);

			return view;
		}

		void Save_Click(object sender, EventArgs e)
		{

			using (var trans = MainDatabase.BeginTransaction()) {
				var message = MainDatabase.Create<Entities.Message>();
				message.Text = Text.Text;
				trans.Commit();
			}

			OnAfterSaved(EventArgs.Empty);

			Dismiss();
		}

	}
}

