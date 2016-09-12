
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

namespace CRMLite
{
	[Activity(Label = "RouteActivity", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape)]
	public class RouteActivity : Activity
	{
		public const int C_ITEMS_IN_RESULT = 10;

		List<SearchItem> SearchItems;
		List<SearchItem> SearchedItems;
		ListView PharmacyTable;
		LinearLayout RouteTable;

		ViewSwitcher SearchSwitcher;

		ImageView SearchImage;

		EditText SearchEditor;

		Animator card_flip_right_in;
		Animator card_flip_right_out;
		Animator card_flip_left_in;
		Animator card_flip_left_out;

		Animator card_flip_up_out;
		Animator card_flip_up_in;

		//Animator cardFlipUpOutForClickedView;
		//Animator cardFlipUpOutForSwapedView;
		//Animator cardFlipUpInForClickedView;
		//Animator cardFlipUpInForSwapedView;

		//LinearLayout Animated;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
    
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			card_flip_right_in = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_right_in);
			card_flip_right_out = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_right_out);

			card_flip_left_in = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_left_in);
			card_flip_left_out = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_left_out);;

			card_flip_up_out = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_up_out);
			card_flip_up_in = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_up_in);

			//cardFlipUpOutForClickedView = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_up_out);
			//cardFlipUpOutForSwapedView = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_up_out);;

			//cardFlipUpInForClickedView = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_up_in);
			//cardFlipUpInForSwapedView = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_up_in);

			// Create your application here
			SetContentView(Resource.Layout.Route);

			PharmacyTable = FindViewById<ListView>(Resource.Id.raPharmacyTable);
			PharmacyTable.ItemClick += (sender, e) => {
				var row = LayoutInflater.Inflate(Resource.Layout.RoutePharmacyItem, RouteTable, false);

				SearchItem item;
				if (string.IsNullOrEmpty(SearchEditor.Text)) {
					item = SearchItems[e.Position];
				} else {
					item = SearchedItems[e.Position];
				}
				row.FindViewById<TextView>(Resource.Id.sriPharmacyTV).Text = item.Name;
				row.SetTag(Resource.String.RouteItemOrder, RouteTable.ChildCount);
				//row.Click += Row_Click
				row.LongClick += (caller, args) => {
					if (caller is LinearLayout) {
						var view = caller as LinearLayout;
						var index = (int)view.GetTag(Resource.String.RouteItemOrder);

						//var data = ClipData.NewPlainText(@"data", @"my_data");
						var data = ClipData.NewPlainText(@"RouteItemOrder", index.ToString());
						//var shadow = new MyShadowBuilder(view);
						var shadow = new View.DragShadowBuilder(view);
						view.StartDrag(data, shadow, null, 0);
					}
				};

				row.Drag += (caller, args) => {
					if (caller is LinearLayout) {
						var view = caller as LinearLayout;
						switch (args.Event.Action) {
							case DragAction.Started:
								args.Handled = true;
								break;
							case DragAction.Entered:
								view.Visibility = ViewStates.Invisible;
								break;
							case DragAction.Exited:
								view.Visibility = ViewStates.Visible;
								break;
							case DragAction.Ended:
								view.Visibility = ViewStates.Visible;
								args.Handled = true;
								break;
							case DragAction.Drop:
								int clipedIndex = int.Parse(args.Event.ClipData.GetItemAt(0).Text);
								var index = (int)view.GetTag(Resource.String.RouteItemOrder);
								if (clipedIndex != index) {
									var dragedView = RouteTable.GetChildAt(clipedIndex);
									RouteTable.RemoveView(dragedView);
									RouteTable.RemoveView(view);
									RouteTable.AddView(dragedView, index);
									RouteTable.AddView(view, clipedIndex);

									dragedView.SetTag(Resource.String.RouteItemOrder, index);
									view.SetTag(Resource.String.RouteItemOrder, clipedIndex);
								}
								view.Visibility = ViewStates.Visible;
								args.Handled = true;
								break;
						}
					}
				};

				RouteTable.AddView(row);
			};

			SearchItems = new List<SearchItem>();
			var pharmacies = MainDatabase.GetItems<Pharmacy>();
			foreach (var item in pharmacies) {
				SearchItems.Add(
					new SearchItem(
						item.UUID,
						item.GetName(),
						MainDatabase.GetItem<Subway>(item.Subway).name,
						MainDatabase.GetItem<Entities.Region>(item.Region).name,
						item.Brand
					)
				);
			}
			SearchedItems = new List<SearchItem>();
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
					foreach (var item in SearchItems) {
						item.Match = string.Empty; 
					}
					PharmacyTable.Adapter = new RoutePharmacyAdapter(this, SearchItems);
					return;
				}

				var w = new Stopwatch();
				w.Start();
				SearchedItems = new List<SearchItem>();
				var matchFormat = @"Совпадение: {0}";
				var culture = CultureInfo.GetCultureInfo("ru-RU");
				// 2 поиск
				foreach (var item in SearchItems) {
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
				w.Stop();
				Console.WriteLine(@"Search: поиск={0}", w.ElapsedMilliseconds);

				PharmacyTable.Adapter = new RoutePharmacyAdapter(this, SearchedItems);
			};

			RouteTable = FindViewById<LinearLayout>(Resource.Id.raRouteTable);
		}

		void Row_Click(object sender, EventArgs e)
		{
			if (sender is LinearLayout) {
				var cardFlipUpOutForClickedView = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_up_out);
				var cardFlipUpOutForSwapedView = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_up_out);

				var cardFlipUpInForClickedView = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_up_in);
				var cardFlipUpInForSwapedView = AnimatorInflater.LoadAnimator(this, Resource.Animation.card_flip_up_in);

				var clickedView = ((LinearLayout)sender);
				var index = (int) clickedView.GetTag(Resource.String.RouteItemOrder);
				if (index < 1) return;

				var swapedView = RouteTable.GetChildAt(index - 1);

				cardFlipUpOutForClickedView.SetTarget(clickedView);
				cardFlipUpOutForClickedView.Start();
				cardFlipUpOutForSwapedView.SetTarget(swapedView);
				cardFlipUpOutForSwapedView.Start();
				cardFlipUpOutForClickedView.AnimationEnd += (c, a) => {
					RouteTable.RemoveView(clickedView);
					RouteTable.AddView(clickedView, index - 1);
					clickedView.SetTag(Resource.String.RouteItemOrder, index - 1);
					swapedView.SetTag(Resource.String.RouteItemOrder, index);
					cardFlipUpInForSwapedView.SetTarget(swapedView);
					cardFlipUpInForSwapedView.Start();
					cardFlipUpInForClickedView.SetTarget(clickedView);
					cardFlipUpInForClickedView.Start();
				};


				//((LinearLayout)caller)s
				//card_flip_left_in.SetTarget((Java.Lang.Object)caller);
				//card_flip_left_in.Start();

				//card_flip_left_out.SetTarget((Java.Lang.Object)caller);
				//card_flip_left_out.Start()
				//card_flip_right_out.SetTarget(Animated);
				//card_flip_right_out.Start();
				//card_flip_right_in.SetTarget(Animated);
				//card_flip_right_in.Start();
				//card_flip_right_out.AnimationEnd += Card_Flip_Right_Out_AnimationEnd;

				//card_flip_out_out.SetTarget(Animated);
				//card_flip_out_out.Start()


			}
		}

		void Card_Flip_Right_Out_AnimationEnd(object sender, EventArgs e)
		{
			//card_flip_right_in.SetTarget(Animated);
			//card_flip_right_in.Start();
			Console.WriteLine("Animation");
		}

		protected override void OnResume()
		{
			base.OnResume();

			PharmacyTable.Adapter = new RoutePharmacyAdapter(this, SearchItems);
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

