using System;
using System.Linq;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using CRMLite.DaData;
using Java.Lang;

namespace CRMLite.Adapters
{
	public class AddressSuggestionAdapter: BaseAdapter<SuggestAddressResponse.Suggestions>, IFilterable
	{
		readonly Activity Context;
		readonly IList<SuggestAddressResponse.Suggestions> Suggestions;

		Filter Dummy;

		public AddressSuggestionAdapter(Activity context, IList<SuggestAddressResponse.Suggestions> suggestions)
		{
			Context = context;
			Suggestions = suggestions;
		}

		public override SuggestAddressResponse.Suggestions this[int position] {
			get {
				return Suggestions[position];
			}
		}

		public override int Count {
			get {
				return Suggestions.Count;
			}
		}

		public Filter Filter {
			get {
				if (Dummy == null) {
					Dummy = new DummyFilter(Suggestions);
				}
				return Dummy;
			}
		}

		public class DummyFilter : Filter
		{
			readonly string[] Suggestions;

			public DummyFilter(IList<SuggestAddressResponse.Suggestions> suggestions)
			{
				Suggestions = suggestions.Select(x => x.value).ToArray(); ;
			}

			protected override FilterResults PerformFiltering(ICharSequence constraint)
			{
				var result = new FilterResults();
				result.Values = Suggestions;
				result.Count = Suggestions.Count();

				return result;
			}

			protected override void PublishResults(ICharSequence constraint, FilterResults results)
			{
				return;		
			}
		}


		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for positio
			var item = Suggestions[position];

			var view = (convertView ?? Context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleDropDownItem1Line, parent, false)
			           ) as TextView;

			view.Text = item.value;

			return view;
		}
	}
}
