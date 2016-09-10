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
	public class FilterDialog : DialogFragment
	{
		public const string TAG = @"FilterDialog";
		public const string C_FILTER_PREFS = @"CRMLITE_FILTER";

		public event EventHandler AfterSaved;

		IList<Subway> Subways;
		AutoCompleteTextView Subway;
		IList<Region> Regions;
		AutoCompleteTextView Region;
		IList<Net> Nets;
		AutoCompleteTextView Net;

		EditText Brand;

		protected virtual void OnAfterSaved(EventArgs e)
		{
			if (AfterSaved != null) {
				AfterSaved(this, e);
			}
		}

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
			Dialog.SetTitle(@"Настройка фильтра");
			Dialog.Window.RequestFeature(WindowFeatures.NoTitle);

			View view = inflater.Inflate(Resource.Layout.FilterDialog, container, false);

			var prefs = Activity.GetSharedPreferences(C_FILTER_PREFS, FileCreationMode.Private);
			Subway = view.FindViewById<AutoCompleteTextView>(Resource.Id.fdSubwayACTV);
			string subwayUUID = prefs.GetString(@"SUBWAY", string.Empty);
			if (!string.IsNullOrEmpty(subwayUUID)) {
				Subway.Text = MainDatabase.GetItem<Subway>(subwayUUID).name;
				Subway.SetTag(Resource.String.SubwayUUID, subwayUUID);
			}
			Region = view.FindViewById<AutoCompleteTextView>(Resource.Id.fdRegionACTV);
			string regionUUID = prefs.GetString(@"REGION", string.Empty);
			if (!string.IsNullOrEmpty(regionUUID)) {
				Region.Text = MainDatabase.GetItem<Region>(regionUUID).name;
				Region.SetTag(Resource.String.RegionUUID, regionUUID);
			}
			Net = view.FindViewById<AutoCompleteTextView>(Resource.Id.fdNetACTV);
			string netUUID = prefs.GetString(@"NET", string.Empty);
			if (!string.IsNullOrEmpty(netUUID)) {
				Net.Text = MainDatabase.GetItem<Net>(netUUID).name;
				Net.SetTag(Resource.String.NetUUID, netUUID);
			}
			Brand = view.FindViewById<EditText>(Resource.Id.fdBrandET);
			Brand.Text = prefs.GetString(@"BRAND", string.Empty);

			InitViews();

			var close = view.FindViewById<Button>(Resource.Id.Close);
			close.Click += (sender, e) => {
				Dismiss();
			};

			var save = view.FindViewById<Button>(Resource.Id.Save);
			save.Click += Save_Click;

			return view;
		}

		void Save_Click(object sender, EventArgs e)
		{
			var edit = Activity.GetSharedPreferences(C_FILTER_PREFS, FileCreationMode.Private).Edit();
			if (string.IsNullOrEmpty(Subway.Text)) {
				edit.PutString(@"SUBWAY", string.Empty);
			} else {
				var subwayUUID = (string)Subway.GetTag(Resource.String.SubwayUUID);
				if (!string.IsNullOrEmpty(subwayUUID)) {
					edit.PutString(@"SUBWAY", subwayUUID);
				}
			}

			if (string.IsNullOrEmpty(Region.Text)) {
				edit.PutString(@"REGION", string.Empty);
			} else {
				var regionUUID = (string)Region.GetTag(Resource.String.RegionUUID);
				if (!string.IsNullOrEmpty(regionUUID)) {
					edit.PutString(@"REGION", regionUUID);
				}
			}

			if (string.IsNullOrEmpty(Net.Text)) {
				edit.PutString(@"NET", string.Empty);
			} else {
				var netUUID = (string)Net.GetTag(Resource.String.NetUUID);
				if (!string.IsNullOrEmpty(netUUID)) {
					edit.PutString(@"NET", netUUID);
				}
			}

			edit.PutString(@"BRAND", Brand.Text);

			edit.Commit();

			OnAfterSaved(EventArgs.Empty);

			Dismiss();
		}

		void InitViews()
		{
			#region Subway
			Subways = MainDatabase.GetItems<Subway>();
			Subway.Adapter = new ArrayAdapter<string>(
				Activity, Android.Resource.Layout.SimpleDropDownItem1Line, Subways.Select(s => s.name).ToArray()
			);
			Subway.ItemClick += (sender, e) => {
				if (sender is AutoCompleteTextView) {
					var text = ((AutoCompleteTextView)sender).Text;
					var subway = Subways.SingleOrDefault(s => s.name.Equals(text));
					if (subway != null) {
						((AutoCompleteTextView)sender).SetTag(Resource.String.SubwayUUID, subway.uuid);
					}
				}
			};
			#endregion

			#region Region
			Regions = MainDatabase.GetItems<Region>();
			Region.Adapter = new ArrayAdapter<string>(
				Activity, Android.Resource.Layout.SimpleDropDownItem1Line, Regions.Select(r => r.name).ToArray()
			);
			Region.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				if (sender is AutoCompleteTextView) {
					var text = ((AutoCompleteTextView)sender).Text;
					var region = Regions.SingleOrDefault(r => r.name.Equals(text));
					if (region != null) {
						((AutoCompleteTextView)sender).SetTag(Resource.String.RegionUUID, region.uuid);
					}
				}
			};
			#endregion

			#region Net
			Nets = MainDatabase.GetItems<Net>();
			Net.Adapter = new ArrayAdapter<string>(
				Activity, Android.Resource.Layout.SimpleDropDownItem1Line, Nets.Select(r => r.name).ToArray()
			);
			Net.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				if (sender is AutoCompleteTextView) {
					var text = ((AutoCompleteTextView)sender).Text;
					var net = Nets.SingleOrDefault(n => n.name.Equals(text));
					if (net != null) {
						((AutoCompleteTextView)sender).SetTag(Resource.String.NetUUID, net.uuid);
					}
				}
			};
			#endregion
		}
	}
}

