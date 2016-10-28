using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.Entities;

namespace CRMLite.Adapters
{
	public class LibraryAdapter : BaseAdapter<LibraryFile>
	{
		readonly Activity Context;
		readonly IList<LibraryFile> LibraryFiles;

		public LibraryAdapter(Activity context, IList<LibraryFile> files)
		{
			Context = context;
			LibraryFiles = files;
		}

		public override LibraryFile this[int position] {
			get {
				return LibraryFiles[position];
			}
		}

		public override int Count {
			get {
				return LibraryFiles.Count;
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for positio
			var item = LibraryFiles[position];

			var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.LibraryTableItem, parent, false)
					   ) as LinearLayout;

			//view.SetTag(Resource.String.ContractDataUUID, ContractDatas[position].UUID);

			view.FindViewById<TextView>(Resource.Id.ltiFileNameTV).Text = item.fileName;
			view.FindViewById<TextView>(Resource.Id.ltiDescriptionTV).Text = item.description;

			return view;
		}
	}
}
