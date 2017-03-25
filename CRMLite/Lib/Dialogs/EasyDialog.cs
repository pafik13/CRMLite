
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace CRMLite.Lib.Dialogs
{
	public enum EasyDialogGravity
	{
		TOP,    /* Содержание выше треугольника */
		BOTTOM, /* Содержание ниже треугольника */
		LEFT,   /* Содержание левее треугольника */
		RIGHT   /* Содержание правее треугольника */
	};

	public enum EasyDialogAnimType
	{
		SHOW, /* Содержание выше треугольника */
		HIDE, /* Содержание ниже треугольника */
	};

	public enum EasyDialogDirection
	{
		X, /* Содержание выше треугольника */
		Y, /* Содержание ниже треугольника */
	};

	public class EasyDialog : Java.Lang.Object, View.IOnTouchListener, ViewTreeObserver.IOnGlobalLayoutListener
	{
		public bool OnTouch(View v, MotionEvent e)
		{
			if (TouchOutsideDismiss && Dialog != null) {
				OnDialogDismiss();
			}
			return false;
		}

		public void OnGlobalLayout()
		{
			Relocation(Location);
		}

		const string TAG = "CRMLite.Lib.Dialogs.EasyDialog";

		Context Context;

		/**
		 * Экземпляр диалога 
		 */
		public Dialog Dialog { get; private set; }

		/**
		 * Координаты
		 */
		int[] _location;
		public int[] Location {
			get { return _location; }
			set { _location = value; }
		}

		/**
		 * Расположение содержания
		 */
		EasyDialogGravity _gravity;
		public EasyDialogGravity Gravity {
			get { return _gravity; }
			set {
				_gravity = value;
				switch (value) {
					case EasyDialogGravity.BOTTOM:
						Triangle.SetBackgroundResource(Resource.Drawable.triangle_bottom);
						break;
					case EasyDialogGravity.TOP:
						Triangle.SetBackgroundResource(Resource.Drawable.triangle_top);
						break;
					case EasyDialogGravity.LEFT:
						Triangle.SetBackgroundResource(Resource.Drawable.triangle_left);
						break;
					case EasyDialogGravity.RIGHT:
						Triangle.SetBackgroundResource(Resource.Drawable.triangle_right);
						break;
				}
				Content.SetBackgroundResource(Resource.Drawable.round_corner_bg);
				if (AttachedView != null) // TODO: како-то косячок?
				{
					SetLocationByAttachedView(AttachedView);
				}
				//SetBackgroundColor(BackgroundColor); // TODO: како-то косячок?
			}
		}

		/**
		 * Переданное извне View
		 */
		View ContentView;
		/**
		 * Треугольник
		 */
		ImageView Triangle;
		/**
		 * Какая-то глушилка
		 */
		LinearLayout Content;
		/**
		 * Необходимо ли закрывать диалог при нажатии вне его
		 */
		bool _touchOutsideDismiss;
		public bool TouchOutsideDismiss {
			get { return _touchOutsideDismiss; }
			set {
				_touchOutsideDismiss = value;
				if (value) {
					OutsideBackground.SetOnTouchListener(this);
				} else {
					OutsideBackground.SetOnTouchListener(null);
				}
			}
		}

		/**
		 * Необходимо, когда нет contentView
		 */
		RelativeLayout OutsideBackground;

		/**
		 * Анимации 
		 */
		AnimatorSet AnimatorSetForDialogShow;
		AnimatorSet AnimatorSetForDialogDismiss;
		List<Animator> ObjectAnimatorsForDialogShow;
		List<Animator> ObjectAnimatorsForDialogDismiss;

		int DefaultLeftMargin;
		int DefaultRightMargin;

		public EasyDialog(Context context)
		{
			InitDialog(context);
		}

		void InitDialog(Context context)
		{
			Context = context;
			LayoutInflater layoutInflater = ((Activity)context).LayoutInflater;
			View dialogView = layoutInflater.Inflate(Resource.Layout.EasyDialog, null);
			dialogView.ViewTreeObserver.AddOnGlobalLayoutListener(this);


			OutsideBackground = dialogView.FindViewById<RelativeLayout>(Resource.Id.edOutsideBackgroundRL);

			//SetTouchOutsideDismiss(true);
			Triangle = dialogView.FindViewById<ImageView>(Resource.Id.edTriangleIV);
			Content = dialogView.FindViewById<LinearLayout>(Resource.Id.edContentLL);
			int theme = IsFullScreen() ? Android.Resource.Style.ThemeTranslucentNoTitleBarFullScreen : Android.Resource.Style.ThemeTranslucentNoTitleBar;
			Dialog = new Dialog(context, theme);
			Dialog.SetContentView(dialogView);
			//        dialog.setOnDismissListener(new DialogInterface.OnDismissListener()
			//        {
			//            @Override
			//			public void onDismiss(DialogInterface dialog)
			//	{
			//		if (onEasyDialogDismissed != null) {
			//			onEasyDialogDismissed.onDismissed();
			//		}
			//	}
			//});
			//        dialog.setOnShowListener(new DialogInterface.OnShowListener()
			//        {
			//            @Override
			//			public void onShow(DialogInterface dialog)
			//{
			//	if (onEasyDialogShow != null) {
			//		onEasyDialogShow.onShow();
			//	}
			//}
			//        });
			AnimatorSetForDialogShow = new AnimatorSet();
			AnimatorSetForDialogDismiss = new AnimatorSet();
			ObjectAnimatorsForDialogShow = new List<Animator>();
			ObjectAnimatorsForDialogDismiss = new List<Animator>();

			DefaultLeftMargin = context.Resources.GetDimensionPixelOffset(Resource.Dimension.easy_dialog_default_left_margin);
			DefaultRightMargin = context.Resources.GetDimensionPixelOffset(Resource.Dimension.easy_dialog_default_right_margin);

			DefaultValues();
		}

		/**
		 * Инициализация по умолчанию
		 */
		void DefaultValues()
		{
			Location = new int[] { 0, 0 };
			Gravity = EasyDialogGravity.BOTTOM;
			TouchOutsideDismiss = true;
			SetOutsideColor(Android.Graphics.Color.Transparent);
			SetBackgroundColor(Android.Graphics.Color.Blue);
			SetMatchParent(true);
			SetMarginLeftAndRight(DefaultLeftMargin, DefaultRightMargin);
		}

		public void SetOutsideColor(Android.Graphics.Color color)
		{
			OutsideBackground.SetBackgroundColor(color);
		}

		/**
		 * http://stackoverflow.com/questions/24492000/set-color-of-triangle-on-run-time
		 * http://stackoverflow.com/questions/16636412/change-shape-solid-color-at-runtime-inside-drawable-xml-used-as-background
		 */
		public void SetBackgroundColor(Android.Graphics.Color color)
		{
			var drawableTriangle = (LayerDrawable)Triangle.Background;
			var shapeTriangle = (GradientDrawable)(((RotateDrawable)drawableTriangle.FindDrawableByLayerId(Resource.Id.shape_id)).Drawable);
			if (shapeTriangle != null) {
				shapeTriangle.SetColor(color);
			} else {
				Toast.MakeText(Context, "shape is null", ToastLength.Short).Show();
			}
			var drawableRound = (GradientDrawable)Content.Background;
			if (drawableRound != null) {
				drawableRound.SetColor(color);
			}
		}

		public void SetMatchParent(bool matchParent)
		{
			var layoutParams = Content.LayoutParameters;
			layoutParams.Width = matchParent ? ViewGroup.LayoutParams.MatchParent : ViewGroup.LayoutParams.WrapContent;
			Content.LayoutParameters = layoutParams;
		}

		public void SetMarginLeftAndRight(int left, int right)
		{
			var layoutParams = (RelativeLayout.LayoutParams)Content.LayoutParameters;
			layoutParams.SetMargins(left, 0, right, 0);
			Content.LayoutParameters = layoutParams;
		}

		public void SetMarginTopAndBottom(int top, int bottom)
		{
			var layoutParams = (RelativeLayout.LayoutParams)Content.LayoutParameters;
			layoutParams.SetMargins(0, top, 0, bottom);
			Content.LayoutParameters = layoutParams;
		}

		View AttachedView;

		/**
		 * @param attachedView 
		 */
		public void SetLocationByAttachedView(View attachedView)
		{
			if (attachedView != null) {
				AttachedView = attachedView;
				int[] attachedViewLocation = new int[2];
				attachedView.GetLocationOnScreen(attachedViewLocation);
				switch (Gravity) {
					case EasyDialogGravity.BOTTOM:
						attachedViewLocation[0] += attachedView.Width / 2;
						attachedViewLocation[1] += attachedView.Height;
						break;
					case EasyDialogGravity.TOP:
						attachedViewLocation[0] += attachedView.Width / 2;
						break;
					case EasyDialogGravity.LEFT:
						attachedViewLocation[1] += attachedView.Height / 2;
						break;
					case EasyDialogGravity.RIGHT:
						attachedViewLocation[0] += attachedView.Width;
						attachedViewLocation[1] += attachedView.Height / 2;
						break;
				}
				Location = attachedViewLocation;
			}
		}

		public void SetCancelable(bool cancelable)
		{
			Dialog.SetCancelable(cancelable);
		}


		/**
		 */
		public bool IsFullScreen()
		{
			var flags = (int)(Context as Activity).Window.Attributes.Flags;
			//bool flag = false || (flags & 1024) == 1024;
			return (false || (flags & 1024) == 1024);
		}

		/**
		 */
		int GetStatusBarHeight()
		{
			int result = 0;
			int resourceId = Context.Resources.GetIdentifier("status_bar_height", "dimen", "android");
			if (resourceId > 0) {
				result = Context.Resources.GetDimensionPixelSize(resourceId);
			}
			return result;
		}


		/**
		 */
		int GetScreenWidth()
		{
			return Context.Resources.DisplayMetrics.WidthPixels;
		}

		int GetScreenHeight()
		{
			int statusBarHeight = IsFullScreen() ? 0 : GetStatusBarHeight();
			return Context.Resources.DisplayMetrics.HeightPixels - statusBarHeight;
		}


		/**
		 */
		void Relocation(int[] location)
		{
			float statusBarHeight = IsFullScreen() ? 0.0f : GetStatusBarHeight();

			Triangle.SetX(location[0] - Triangle.Width / 2);
			Triangle.SetY(location[1] - Triangle.Height / 2 - statusBarHeight);
			switch (Gravity) {
				case EasyDialogGravity.BOTTOM:
					Content.SetY(location[1] - Triangle.Height / 2 - statusBarHeight + Triangle.Height);
					break;
				case EasyDialogGravity.TOP:
					Content.SetY(location[1] - Content.Height - statusBarHeight - Triangle.Height / 2);
					break;
				case EasyDialogGravity.LEFT:
					Content.SetX(location[0] - Content.Width - Triangle.Width / 2);
					break;
				case EasyDialogGravity.RIGHT:
					Content.SetX(location[0] + Triangle.Width / 2);
					break;
			}

			var layoutParams = (RelativeLayout.LayoutParams)Content.LayoutParameters;
			switch (Gravity) {
				case EasyDialogGravity.BOTTOM:
				case EasyDialogGravity.TOP:
					var triangleCenterX = (int)(Triangle.GetX() + Triangle.Width / 2);
					var contentWidth = Content.Width;
					var rightMargin = GetScreenWidth() - triangleCenterX;
					var leftMargin = GetScreenWidth() - rightMargin;
					var availableLeftMargin = leftMargin - layoutParams.LeftMargin;
					var availableRightMargin = rightMargin - layoutParams.RightMargin;
					int x = 0;
					if (contentWidth / 2 <= availableLeftMargin && contentWidth / 2 <= availableRightMargin) {
						x = triangleCenterX - contentWidth / 2;
					} else {
						if (availableLeftMargin <= availableRightMargin) {
							x = layoutParams.LeftMargin;
						} else {
							x = GetScreenWidth() - (contentWidth + layoutParams.RightMargin);
						}
					}
					Content.SetX(x);
					break;
				case EasyDialogGravity.LEFT:
				case EasyDialogGravity.RIGHT:
					int triangleCenterY = (int)(Triangle.GetY() + Triangle.Height / 2);
					int contentHeight = Content.Height;
					int topMargin = triangleCenterY;
					int bottomMargin = GetScreenHeight() - topMargin;
					int availableTopMargin = topMargin - layoutParams.TopMargin;
					int availableBottomMargin = bottomMargin - layoutParams.BottomMargin;
					int y = 0;
					if (contentHeight / 2 <= availableTopMargin && contentHeight / 2 <= availableBottomMargin) {
						y = triangleCenterY - contentHeight / 2;
					} else {
						if (availableTopMargin <= availableBottomMargin) {
							y = layoutParams.TopMargin;
						} else {
							y = GetScreenHeight() - (contentHeight + layoutParams.TopMargin);
						}
					}
					Content.SetY(y);
					break;
			}
		}


		/**
		 */
		public void Dismiss()
		{
			if (Dialog != null && Dialog.IsShowing) {
				OnDialogDismiss();
			}
		}


		void OnDialogDismiss()
		{
			if (AnimatorSetForDialogDismiss.IsRunning) {
				return;
			}
			if (AnimatorSetForDialogDismiss != null && ObjectAnimatorsForDialogDismiss != null && ObjectAnimatorsForDialogDismiss.Count > 0) {
				AnimatorSetForDialogDismiss.PlayTogether(ObjectAnimatorsForDialogDismiss);
				AnimatorSetForDialogDismiss.Start();
				AnimatorSetForDialogDismiss.AnimationEnd += (sender, e) => {
					if (Context is Activity) {
						if (!(Context as Activity).IsDestroyed) {
							Dialog.Dismiss();
						}
					}
				};
			} else {
				Dialog.Dismiss();
			}
		}

		/**
		 */
		public void SetAnimationAlphaShow(int duration, float[] values)
		{
			SetAnimationAlpha(EasyDialogAnimType.SHOW, duration, values);
		}

		/**
		 */
		public void SetAnimationAlphaHide(int duration, float[] values)
		{
			SetAnimationAlpha(EasyDialogAnimType.HIDE, duration, values);
		}

		void SetAnimationAlpha(EasyDialogAnimType animType, int duration, float[] values)
		{
			var targetForAnimate = OutsideBackground.FindViewById(Resource.Id.edParentForAnimateRL);
			var animator = ObjectAnimator.OfFloat(targetForAnimate, "alpha", values).SetDuration(duration);
			switch (animType) {
				case EasyDialogAnimType.SHOW:
					ObjectAnimatorsForDialogShow.Add(animator);
					break;
				case EasyDialogAnimType.HIDE:
					ObjectAnimatorsForDialogDismiss.Add(animator);
					break;
				default:
					Log.Error(TAG, "Method[SetAnimationAlpha]:EasyDialogAnimType case default");
					break;
			}
		}

		void OnDialogShowing()
		{
			if (AnimatorSetForDialogShow != null && ObjectAnimatorsForDialogShow != null && ObjectAnimatorsForDialogShow.Count > 0) {
				AnimatorSetForDialogShow.PlayTogether(ObjectAnimatorsForDialogShow);
				AnimatorSetForDialogShow.Start();
			}
			//TODO 缩放的动画效果不好，不能从控件所在的位置开始缩放
			//        ObjectAnimator.ofFloat(rlOutsideBackground.findViewById(R.id.rlParentForAnimate), "scaleX", 0.3f, 1.0f).setDuration(500).start();
			//        ObjectAnimator.ofFloat(rlOutsideBackground.findViewById(R.id.rlParentForAnimate), "scaleY", 0.3f, 1.0f).setDuration(500).start();
		}


		/**
		 */
		public EasyDialog Show()
		{
			if (Dialog != null) {
				if (ContentView == null) {
					throw new NullReferenceException("Content view cannot be null");
				}
				if (Content.ChildCount > 0) {
					Content.RemoveAllViews();
				}
				Content.AddView(ContentView);
				Dialog.Show();
				OnDialogShowing();
			}
			return this;
		}

		/**
		 */
		public void SetContent(View contentView)
		{
			if (contentView != null) {
				ContentView = contentView;
			}
		}


		/**
		 */
		public void SetContentByResourceId(int resourceId)
		{
			View view = ((Activity)Context).LayoutInflater.Inflate(resourceId, null);
			SetContent(view);
		}


		/**
		 */
		public View GetTipViewInstance()
		{
			return OutsideBackground.FindViewById(Resource.Id.edParentForAnimateRL);
		}

		/**
		 */
		public void SetAnimationTranslationShow(EasyDialogDirection direction, int duration, float[] values)
		{
			SetAnimationTranslation(EasyDialogAnimType.SHOW, direction, duration, values);
		}

		/**
		 */
		public void SetAnimationTranslationHide(EasyDialogDirection direction, int duration, float[] values)
		{
			SetAnimationTranslation(EasyDialogAnimType.HIDE, direction, duration, values);
		}

		private void SetAnimationTranslation(EasyDialogAnimType animType, EasyDialogDirection direction, int duration, float[] values)
		{
			string propertyName = string.Empty;
			switch (direction) {
				case EasyDialogDirection.X:
					propertyName = "translationX";
					break;
				case EasyDialogDirection.Y:
					propertyName = "translationY";
					break;
			}
			var targetForAnimate = OutsideBackground.FindViewById(Resource.Id.edParentForAnimateRL);
			var animator = ObjectAnimator.OfFloat(targetForAnimate, propertyName, values).SetDuration(duration);
			switch (animType) {
				case EasyDialogAnimType.SHOW:
					ObjectAnimatorsForDialogShow.Add(animator);
					break;
				case EasyDialogAnimType.HIDE:
					ObjectAnimatorsForDialogDismiss.Add(animator);
					break;
				default:
					Log.Error(TAG, "Method[SetAnimationTranslation]:EasyDialogAnimType case default");
					break;
			}
		}
	}
}

