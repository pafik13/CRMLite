
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CRMLite
{
	[Activity(Label = "MessageActivity")]
	public class MessageActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here

			SetContentView(Resource.Layout.Message);

			var close = FindViewById<Button>(Resource.Id.maCloseB);
			close.Click += (sender, e) => {
				Finish();
			};

			var messages = MainDatabase.GetItems<Entities.Message>();
			var table = FindViewById<LinearLayout>(Resource.Id.maMessageTable);
			foreach (var item in messages) {
				table.AddView(new TextView(this) { Text = item.Text });     
			}
		}
	}
}

