
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

using CRMLite.Entities;
using CRMLite.Adapters;
using Android.Views.InputMethods;
using System.Globalization;
using System.Diagnostics;
using Android.Views.Animations;
using Android.Animation;
using Android.Graphics;
using Android.Support.V4.App;
using Android.Support.V4.View;

namespace CRMLite
{
	[Activity(Label = "RouteActivity", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape)]
	public class RouteActivity : FragmentActivity, ViewPager.IOnPageChangeListener
	{
		public const int C_ITEMS_IN_RESULT = 10;
		public const int C_FRAGMENTS_COUNT = 5;

		DateTimeOffset SelectedDate;

		List<RouteSearchItem> RouteSearchItemsSource;
		List<RouteSearchItem> RouteSearchItems;
		List<RouteSearchItem> SearchedItems;
		ListView PharmacyTable;
		RoutePharmacyAdapter RoutePharmacyAdapter;
		LinearLayout RouteTable;

		ViewSwitcher SearchSwitcher;

		ImageView SearchImage;

		EditText SearchEditor;

		TextView Info;

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			return;
		}

		public void OnPageScrollStateChanged(int state)
		{
			return;
		}

		public void OnPageSelected(int position)
		{
			switch (position) {
				default:
					Info.Text = string.Format(
						@"Период планирования: {0} недели ({1} дней) Фрагмент {2}",
						Helper.WeeksInRoute, Helper.WeeksInRoute * 5, position
					);
					break; ;
			}
		}


		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
    
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Route);

			FindViewById<Button>(Resource.Id.raCloseB).Click += (s, e) => {
				Finish();
			};

			PharmacyTable = FindViewById<ListView>(Resource.Id.raPharmacyTable);
			PharmacyTable.ItemClick += (sender, e) => {
				var ll = (ListView)sender;
				var adapter = (RoutePharmacyAdapter)ll.Adapter;
				adapter.SwitchVisibility(e.Position);

				var row = LayoutInflater.Inflate(Resource.Layout.RouteItem, RouteTable, false);
				row.SetTag(Resource.String.Position, e.Position);

				RouteSearchItem item;
				if (string.IsNullOrEmpty(SearchEditor.Text)) {
					item = RouteSearchItems[e.Position];
				} else {
					item = SearchedItems[e.Position];
				}
				
				//TODO: rename vars
				using (var trans = MainDatabase.BeginTransaction()){
					var newRouteItem = MainDatabase.Create<RouteItem>();
					newRouteItem.Pharmacy = item.UUID;
					newRouteItem.Order = RouteTable.ChildCount;
					newRouteItem.Date = SelectedDate;
					trans.Commit();
					row.SetTag(Resource.String.RouteItemUUID, newRouteItem.UUID);
				}
				row.SetTag(Resource.String.PharmacyUUID, item.UUID);

				row.FindViewById<TextView>(Resource.Id.riPharmacyTV).Text = item.Name;
				row.SetTag(Resource.String.RouteItemOrder, RouteTable.ChildCount);
				row.FindViewById<TextView>(Resource.Id.riOrderTV).Text = (RouteTable.ChildCount + 1).ToString();

				row.FindViewById<ImageView>(Resource.Id.riDeleteIV).Click += RowDelete_Click;
				row.LongClick += Row_LongClick;
				row.Drag += Row_Drag;

				RouteTable.AddView(row);
			};

			RouteSearchItemsSource = new List<RouteSearchItem>();
			var pharmacies = MainDatabase.GetItems<Pharmacy>();
			foreach (var item in pharmacies) {
				RouteSearchItemsSource.Add(
					new RouteSearchItem(
						item.UUID,
						item.GetName(),
						MainDatabase.GetItem<Subway>(item.Subway).name,
						MainDatabase.GetItem<Entities.Region>(item.Region).name,
						item.Brand
					)
				);
			}
			SearchedItems = new List<RouteSearchItem>();
			SearchSwitcher = FindViewById<ViewSwitcher>(Resource.Id.raSearchVS);
			SearchSwitcher.SetInAnimation(this, Android.Resource.Animation.SlideInLeft);
			SearchSwitcher.SetOutAnimation(this, Android.Resource.Animation.SlideOutRight);

			SearchImage = FindViewById<ImageView>(Resource.Id.raSearchIV);
			SearchImage.Click += (sender, e) => {
				if (CurrentFocus != null) {
					var imm = (InputMethodManager)GetSystemService(InputMethodService);
					imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.None);
				}

				SearchSwitcher.ShowNext();
			};

			SearchEditor = FindViewById<EditText>(Resource.Id.raSearchET);

			SearchEditor.AfterTextChanged += (sender, e) => {
				var text = e.Editable.ToString();

				if (string.IsNullOrEmpty(text)) {
					foreach (var item in RouteSearchItems) {
						item.Match = string.Empty; 
					}
					PharmacyTable.Adapter = new RoutePharmacyAdapter(this, RouteSearchItems);
					return;
				}

				var w = new Stopwatch();
				w.Start();
				SearchedItems = new List<RouteSearchItem>();
				var matchFormat = @"Совпадение: {0}";
				var culture = CultureInfo.GetCultureInfo("ru-RU");
				// 2 поиск
				foreach (var item in RouteSearchItems) {
					if (item.IsVisible) {
						if (culture.CompareInfo.IndexOf(item.Subway, text, CompareOptions.IgnoreCase) >= 0) {
							item.Match = string.Format(matchFormat, @"метро=" + item.Subway);
							SearchedItems.Add(item);
							if (SearchedItems.Count > C_ITEMS_IN_RESULT) break;
							continue;
						}

						if (culture.CompareInfo.IndexOf(item.Region, text, CompareOptions.IgnoreCase) >= 0) {
							item.Match = string.Format(matchFormat, @"район=" + item.Region);
							SearchedItems.Add(item);
							if (SearchedItems.Count > C_ITEMS_IN_RESULT) break;
							continue;
						}

						if (culture.CompareInfo.IndexOf(item.Brand, text, CompareOptions.IgnoreCase) >= 0) {
							item.Match = string.Format(matchFormat, @"бренд=" + item.Brand);
							SearchedItems.Add(item);
							if (SearchedItems.Count > C_ITEMS_IN_RESULT) break;
							continue;
						}
					}
				}
				w.Stop();
				Console.WriteLine(@"Search: поиск={0}", w.ElapsedMilliseconds);

				PharmacyTable.Adapter = new RoutePharmacyAdapter(this, SearchedItems);
			};

			RouteTable = FindViewById<LinearLayout>(Resource.Id.raRouteTable);

			FindViewById<Button>(Resource.Id.raSelectDateB).Click += (sender, e) => {
				DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime date) {
					Console.WriteLine("DatePicker:{0}", date.ToLongDateString());
					Console.WriteLine("DatePicker:{0}", new DateTimeOffset(date));
					SelectedDate = new DateTimeOffset(date, new TimeSpan(0, 0, 0)); ;
					RefreshTables();
				});
				frag.Show(FragmentManager, DatePickerFragment.TAG);
			};

			Info = FindViewById<TextView>(Resource.Id.raInfoTV);
			Info.Text = string.Format(@"Период планирования: {0} недели ({1} дней)", Helper.WeeksInRoute, Helper.WeeksInRoute * 5);

			var switcher = FindViewById<ViewSwitcher>(Resource.Id.raSwitchViewVS);
			FindViewById<ImageView>(Resource.Id.raSwitchIV).Click += (sender, e) => {
				Console.WriteLine(@"switcher:{0}; Resource{1}", switcher.CurrentView.Id, Resource.Id.raContainerVP);
				if (switcher.CurrentView.Id != Resource.Id.raContainerVP) {
					var pager = FindViewById<ViewPager>(Resource.Id.raContainerVP);
					pager.AddOnPageChangeListener(this);
					pager.Adapter = new RoutePagerAdapter(SupportFragmentManager);
				}
				switcher.ShowNext();
			};
		}

		void RowDelete_Click(object sender, EventArgs e)
		{
			var adapter = (RoutePharmacyAdapter)PharmacyTable.Adapter;

			var rowForDelete = (LinearLayout)((ImageView)sender).Parent;

			string routeItemUUID = (string)rowForDelete.GetTag(Resource.String.RouteItemUUID);
			MainDatabase.DeleteEntity<RouteItem>(routeItemUUID);

			int pos = (int) rowForDelete.GetTag(Resource.String.Position);
			int index = (int)rowForDelete.GetTag(Resource.String.RouteItemOrder);

			RouteTable.RemoveView(rowForDelete);

			using (var trans = MainDatabase.BeginTransaction()) {
				for (int c = index; c < RouteTable.ChildCount; c++) {
					var rowForUpdate = (LinearLayout)RouteTable.GetChildAt(c);
					routeItemUUID = (string)rowForUpdate.GetTag(Resource.String.RouteItemUUID);
					var updRouteItem = MainDatabase.GetEntity<RouteItem>(routeItemUUID);
					updRouteItem.Order = c;
					rowForUpdate.SetTag(Resource.String.RouteItemOrder, c);
					rowForUpdate.FindViewById<TextView>(Resource.Id.riOrderTV).Text = (c + 1).ToString();
				}
				trans.Commit();
			}

			adapter.SwitchVisibility(pos);
		}

		void Row_LongClick(object sender, View.LongClickEventArgs e)
		{
			if (sender is LinearLayout) {
				var view = sender as LinearLayout;
				var index = (int)view.GetTag(Resource.String.RouteItemOrder);

				//var data = ClipData.NewPlainText(@"data", @"my_data");
				var data = ClipData.NewPlainText(@"RouteItemOrder", index.ToString());
				//var shadow = new MyShadowBuilder(view);
				var shadow = new View.DragShadowBuilder(view);
				view.StartDrag(data, shadow, null, 0);
			}
		}

		void Row_Drag(object sender, View.DragEventArgs e)
		{
			if (sender is LinearLayout) {
				var view = sender as LinearLayout;
				switch (e.Event.Action) {
					case DragAction.Started:
						e.Handled = true;
						break;
					case DragAction.Entered:
						view.Visibility = ViewStates.Invisible;
						break;
					case DragAction.Exited:
						view.Visibility = ViewStates.Visible;
						break;
					case DragAction.Ended:
						view.Visibility = ViewStates.Visible;
						e.Handled = true;
						break;
					case DragAction.Drop:
						int clipedIndex = int.Parse(e.Event.ClipData.GetItemAt(0).Text);
						var index = (int)view.GetTag(Resource.String.RouteItemOrder);
						if (clipedIndex != index) {
							var dragedView = RouteTable.GetChildAt(clipedIndex);
							RouteTable.RemoveView(dragedView);
							RouteTable.AddView(dragedView, index);
							RouteTable.RemoveView(view);
							RouteTable.AddView(view, clipedIndex);

							using (var trans = MainDatabase.BeginTransaction()) {
								for (int c = 0; c < RouteTable.ChildCount; c++) {
									var rowForUpdate = (LinearLayout)RouteTable.GetChildAt(c);
									string routeItemUUID = (string)rowForUpdate.GetTag(Resource.String.RouteItemUUID);
									var updRouteItem = MainDatabase.GetEntity<RouteItem>(routeItemUUID);
									updRouteItem.Order = c;
									rowForUpdate.SetTag(Resource.String.RouteItemOrder, c);
									rowForUpdate.FindViewById<TextView>(Resource.Id.riOrderTV).Text = (c + 1).ToString();
								}
								trans.Commit();
							}
							//dragedView.SetTag(Resource.String.RouteItemOrder, index);
							//view.SetTag(Resource.String.RouteItemOrder, clipedIndex);
						}
						view.Visibility = ViewStates.Visible;
						e.Handled = true;
						break;
				}
			}
		}

		protected override void OnResume()
		{
			base.OnResume();

			SelectedDate = DateTimeOffset.Now;
			RefreshTables();
		}

		void RefreshTables()
		{
			FindViewById<Button>(Resource.Id.raSelectDateB).Text = SelectedDate.UtcDateTime.Date.ToLongDateString();

			var routeItemsPharmacies = MainDatabase.GetEarlyRouteItems(SelectedDate).Select(ri => ri.Pharmacy);
			RouteSearchItems = RouteSearchItemsSource.Where(rsi => !routeItemsPharmacies.Contains(rsi.UUID)).ToList(); 

			RoutePharmacyAdapter = new RoutePharmacyAdapter(this, RouteSearchItems);
			PharmacyTable.Adapter = RoutePharmacyAdapter;

			for (int i = 0; i < RouteSearchItems.Count; i++) {
				RoutePharmacyAdapter.ChangeVisibility(i, true);
			}

			RouteTable.RemoveAllViews();
			foreach (var item in MainDatabase.GetRouteItems(SelectedDate)) {
				var row = LayoutInflater.Inflate(Resource.Layout.RouteItem, RouteTable, false);
				row.FindViewById<TextView>(Resource.Id.riPharmacyTV).Text = MainDatabase.GetEntity<Pharmacy>(item.Pharmacy).GetName();

				int position = RouteSearchItems.FindIndex(rsi => string.Compare(rsi.UUID, item.Pharmacy) == 0);
				RoutePharmacyAdapter.ChangeVisibility(position, false);

				row.SetTag(Resource.String.Position, position);
				row.SetTag(Resource.String.RouteItemUUID, item.UUID);
				row.SetTag(Resource.String.PharmacyUUID, item.Pharmacy);

				row.SetTag(Resource.String.RouteItemOrder, item.Order);
				row.FindViewById<TextView>(Resource.Id.riOrderTV).Text = (item.Order + 1).ToString();

				row.FindViewById<ImageView>(Resource.Id.riDeleteIV).Click += RowDelete_Click;
				row.LongClick += Row_LongClick;
				row.Drag += Row_Drag;

				RouteTable.AddView(row);
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
		}


		/**
		 * A pager adapter that represents <NUM_PAGES> fragments, in sequence.
		 */
		class RoutePagerAdapter : FragmentPagerAdapter
		{
			public RoutePagerAdapter(Android.Support.V4.App.FragmentManager fm) : base(fm)
			{
			}

			public override int Count {
				get {
					return Helper.WeeksInRoute;
				}
			}

			public override Android.Support.V4.App.Fragment GetItem(int position)
			{
				switch (position) {
					default:
					return RouteFragment.create(position);
				}

			}
		}
	}

	class MyShadowBuilder : View.DragShadowBuilder
	{
		const int centerOffset = 52;
		int width, height;

		public MyShadowBuilder(View baseView) : base(baseView)
		{
		}

		public override void OnProvideShadowMetrics(Point shadowSize, Point shadowTouchPoint)
		{
			width = View.Width;
			height = View.Height;

			// This is the overall dimension of your drag shadow
			shadowSize.Set(width * 2, height * 2);
			// This one tells the system how to translate your shadow on the screen so
			// that the user fingertip is situated on that point of your canvas.
			// In my case, the touch point is in the middle of the (height, width) top-right rect
			shadowTouchPoint.Set(width + width / 2 - centerOffset, height / 2 + centerOffset);
		}

		public override void OnDrawShadow(Canvas canvas)
		{
			const float sepAngle = (float)Math.PI / 16;
			const float circleRadius = 2f;

			// Draw the shadow circles in the top-right corner
			float centerX = width + width / 2 - centerOffset;
			float centerY = height / 2 + centerOffset;

			var baseColor = Color.Black;
			var paint = new Paint() {
				AntiAlias = true,
				Color = baseColor
			};

			// draw a dot where the center of the touch point (i.e. your fingertip) is
			canvas.DrawCircle(centerX, centerY, circleRadius + 1, paint);
			for (int radOffset = 70; centerX + radOffset < canvas.Width; radOffset += 20) {
				// Vary the alpha channel based on how far the dot is
				baseColor.A = (byte)(128 * (2f * (width / 2f - 1.3f * radOffset + 60) / width) + 100);
				paint.Color = baseColor;
				// Draw the dots along a circle of radius radOffset and centered on centerX,centerY
				for (float angle = 0; angle < Math.PI * 2; angle += sepAngle) {
					var pointX = centerX + (float)Math.Cos(angle) * radOffset;
					var pointY = centerY + (float)Math.Sin(angle) * radOffset;
					canvas.DrawCircle(pointX, pointY, circleRadius, paint);
				}
			}

			//View.Dra
			// Draw the dragged view in the bottom-left corner
			//canvas.DrawBitmap(View.DrawingCache, 0, height, null);
		}
	}
}
