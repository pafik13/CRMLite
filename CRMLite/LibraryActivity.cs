using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using Android.Content.PM;

using CRMLite.Adapters;
using CRMLite.Entities;
using Android.Content;
using Android.Webkit;

namespace CRMLite
{
	[Activity(Label = "LibraryActivity", ScreenOrientation = ScreenOrientation.Landscape)]
	public class LibraryActivity : Activity
	{
		public ListView Table { get; private set; }

		public LinearLayout Content { get; private set; }

		List<LibraryFile> Files;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Library);

			FindViewById<Button>(Resource.Id.laCloseB).Click += (s, e) => {
				Finish();
			};

			Table = FindViewById<ListView>(Resource.Id.laLibraryTable);
			var header = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.LibraryTableHeader, Table, false);
			Table.AddHeaderView(header);

			Table.ItemClick += (sender, e) => {
				LibraryFile item;
				if (Table.HeaderViewsCount > 0) {
					if (e.Position < Table.HeaderViewsCount) {
						return;
					}
					item = Files[e.Position - Table.HeaderViewsCount];
				} else {
					item = Files[e.Position];
				}
				var intent = new Intent(Intent.ActionView);
				var file = new Java.IO.File(Helper.LibraryDir, item.fileName);
				var uri = Android.Net.Uri.FromFile(file);
				string extension = System.IO.Path.GetExtension(file.ToString()).Substring(1);
				string mimeType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);

				intent.SetDataAndType(uri, mimeType);
				intent.SetFlags(ActivityFlags.NoHistory);
				StartActivity(intent);
			};
		}

		protected override void OnResume()
		{
			base.OnResume();
			RecreateAdapter();
		}

		void RecreateAdapter()
		{
			Files = MainDatabase.GetItems<LibraryFile>() ?? new List<LibraryFile>();
			Table.Adapter = new LibraryAdapter(this, Files);
		}
	}
}

