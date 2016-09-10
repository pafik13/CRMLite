using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using CRMLite.Entities;

namespace CRMLite
{
	public class SearchResultAdapter : BaseAdapter<SearchItem>
	{
		readonly Activity Context;
		readonly IList<SearchItem> Searched;

		public SearchResultAdapter(Activity context, IList<SearchItem> searched)
		{
			Context = context;
			Searched = searched;
		}

		public override SearchItem this[int position] {
			get { return Searched[position]; }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get { return Searched.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = Searched[position];

			var view = (convertView ??
			            Context.LayoutInflater.Inflate(Resource.Layout.SearchResultItem, parent, false)
					   ) as LinearLayout;

			view.FindViewById<TextView>(Resource.Id.sriPharmacyTV).Text = item.Name;
			view.FindViewById<TextView>(Resource.Id.sriMatchTV).Text = item.Match;

			return view;
		}
	}
}

