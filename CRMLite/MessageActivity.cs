using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using CRMLite.Adapters;
using CRMLite.Dialogs;

namespace CRMLite
{
	[Activity(Label = "MessageActivity")]
	public class MessageActivity : Activity
	{		public ListView Table { get; private set; }

		public LinearLayout Content { get; private set; }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here

			SetContentView(Resource.Layout.Message);

			var close = FindViewById<Button>(Resource.Id.maCloseB);
			close.Click += (sender, e) => {
				Finish();
			};

			Table = FindViewById<ListView>(Resource.Id.maMessageTable);
			Content = FindViewById<LinearLayout>(Resource.Id.maContentLL);
			//var header = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.MessageTableHeader, Table, false);
			//Content.AddView(header, 0);

			var addMsg = FindViewById<ImageView>(Resource.Id.maAddIV);
			addMsg.Click += (sender, e) => {
				var fragmentTransaction = FragmentManager.BeginTransaction();
				var prev = FragmentManager.FindFragmentByTag(MessageDialog.TAG);
				if (prev != null) {
					fragmentTransaction.Remove(prev);
				}
				fragmentTransaction.AddToBackStack(null);

				var messageDialog = new MessageDialog();
				messageDialog.Show(fragmentTransaction, MessageDialog.TAG);
				messageDialog.AfterSaved += (caller, arguments) => {
					Toast.MakeText(this, @"Message saved", ToastLength.Short).Show();
					RecreateAdapter();
				};
			};
		}

		protected override void OnResume()
		{
			base.OnResume();
			RecreateAdapter();
		}

		void RecreateAdapter()
		{
			Table.Adapter = new MessageAdapter(this, MainDatabase.GetItems<Entities.Message>());
		}
}
}

