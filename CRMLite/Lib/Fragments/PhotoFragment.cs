using System.Collections.Generic;
using System.Linq;

using Android.OS;
using Android.Views;
using Android.Widget;

using Android.Support.V4.App;

using CRMLite.Entities;
using CRMLite.Dialogs;
using System;
using Android.Content;
using Android.Media;
using Android.Provider;
using System.IO;
using Realms;

namespace CRMLite
{
	public class PhotoFragment : Fragment, IAttendanceControl
	{

		public const string C_PHARMACY_UUID = @"C_PHARMACY_UUID";
		public const int C_REQUEST_PHOTO = 100;

		LayoutInflater Inflater;

		Pharmacy Pharmacy;
		DateTimeOffset? AttendanceStart;
		TextView Locker;

		//User user = null;
		List<PhotoType> PhotoTypes;
		Spinner PhotoType;
		//List<PhotoSubType> photoSubTypes = null;
		List<DrugBrand> Brands;
		Spinner Brand;
		Button AddPhoto;
		LinearLayout PhotoTable;
		static Java.IO.File file;
		List<Photo> Photos;
		//Dictionary<int, PhotoType> photoTypesDict = null;
		//Dictionary<int, PhotoSubType> photoSubTypesDict = null;

		public static string PhotoDir {
			get {
				return Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, @"MyTempDir");
			}
		}

		/**
		 * Factory method for this fragment class. Constructs a new fragment for the given page number.
		 */
		public static PhotoFragment create(string UUID)
		{
			PhotoFragment fragment = new PhotoFragment();
			Bundle arguments = new Bundle();
			arguments.PutString(C_PHARMACY_UUID, UUID);
			fragment.Arguments = arguments;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var pharmacyUUID = Arguments.GetString(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return;

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);

			Inflater = inflater;

			// Inflate the layout containing a title and body text.
			View view = inflater.Inflate(Resource.Layout.PhotoFragment, container, false);


			//PhotoTypes = MainDatabase.GetItems<PhotoType>();
			//photoTypesDict = new Dictionary<int, PhotoType>();
			//foreach (var item in photoTypes) {
			//	photoTypesDict.Add(item.id, item);
			//}
			//photoSubTypes = Common.GetPhotoSubTypes(user.username);
			//photoSubTypesDict = new Dictionary<int, PhotoSubType>();
			//foreach (var item in photoSubTypes) {
			//	photoSubTypesDict.Add(item.id, item);
			//}

			//newAttendancePhotos = AttendancePhotoManager.GetCurrentAttendancePhotos();
			if (Photos == null) {
				Photos = new List<Photo>();
			}

			PhotoTable = view.FindViewById<LinearLayout>(Resource.Id.pfPhotoTableLL);
			RefreshPhotoList();

			PhotoType = view.FindViewById<Spinner>(Resource.Id.pfPhotoTypeS);
			PhotoTypes = MainDatabase.GetItems<PhotoType>();
			var adapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerItem, PhotoTypes.Select(i => i.name).ToArray());
			adapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			PhotoType.Adapter = adapter;

			//PhotoType.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {
			//	int photoTypesID = photoTypes[e.Position].id;

			//	currentPhotoSubTypes = (List<PhotoSubType>)(from item in photoSubTypes where item.type == photoTypesID select item).ToList();
			//	ArrayAdapter adapterPST = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerItem, (string[])(from item in currentPhotoSubTypes select item.name).ToArray());
			//	adapterPST.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			//	spnPhotoSubTypes.Adapter = adapterPST;

			//};

			Brand = view.FindViewById<Spinner>(Resource.Id.pfBrandS);
			Brands = MainDatabase.GetItems<DrugBrand>();
			var brandAdapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerItem, Brands.Select(i => i.name).ToArray());
			brandAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			Brand.Adapter = brandAdapter;

			AddPhoto = view.FindViewById<Button>(Resource.Id.pfAddPhotoB);
			AddPhoto.Click += (object sender, EventArgs e) => {
			//	if (Common.CreateDirForPhotos(user)) {
			//		string type = photoTypes[spnPhotoTypes.SelectedItemPosition].name;
			//		type = Transliteration.Front(type, TransliterationType.Gost).Substring(0, Math.Min(5, type.Length)).ToUpper();
			//		string subtype = currentPhotoSubTypes[spnPhotoSubTypes.SelectedItemPosition].name;
			//		subtype = Transliteration.Front(subtype, TransliterationType.Gost).Substring(0, Math.Min(5, subtype.Length)).ToUpper();
					string stamp = DateTime.Now.ToString(@"yyyyMMddHHmmsszz");
					file = new Java.IO.File(PhotoDir, string.Format("PHOTO_{0}.jpg", stamp));
					Intent intent = new Intent(MediaStore.ActionImageCapture);
					intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(file));
					StartActivityForResult(intent, C_REQUEST_PHOTO);
			//	}
			};

			Locker = view.FindViewById<TextView>(Resource.Id.locker);

			return view;
		}

		void RefreshPhotoList()
		{
			//llPhotoList.RemoveViews (1, llPhotoList.ChildCount - 1);
			PhotoTable.RemoveAllViews();

			int i = 0;
			foreach (var item in Photos) {
				i++;

				View vPhoto = Inflater.Inflate(Resource.Layout.PhotoTableItem, null);
				vPhoto.SetTag(Resource.String.PhotoPath, item.PhotoPath);
				vPhoto.FindViewById<TextView>(Resource.Id.ptiNumTV).Text = i.ToString();
				var photoType = MainDatabase.GetItem<PhotoType>(item.PhotoType);
				vPhoto.FindViewById<TextView>(Resource.Id.ptiTypeTV).Text = photoType.name;

				string drugBrand = (photoType.isNeedBrand) ? MainDatabase.GetItem<DrugBrand>(item.Drug).name : @"<не требуется>";
				vPhoto.FindViewById<TextView>(Resource.Id.ptiSubTypeTV).Text = drugBrand;

				vPhoto.FindViewById<TextView>(Resource.Id.ptiDateTimeTV).Text = item.Stamp.ToString("G");
				vPhoto.Click += (object sender, EventArgs e) => {
					View view = (View)sender;
					//Toast.MakeText(Activity, view.GetTag(Resource.String.PhotoPath).ToString(), ToastLength.Short).Show();
					Intent intent = new Intent(Intent.ActionView);
					intent.SetDataAndType(Android.Net.Uri.Parse("file://" + view.GetTag(Resource.String.PhotoPath).ToString()), "image/*");
					StartActivity(intent);
				};
				PhotoTable.AddView(vPhoto);
			}
		}

		float convertToDegree(string stringDMS)
		{
			float result = 0.0f;
			if (string.IsNullOrEmpty(stringDMS)) {
				return result;
			} else {
				char[] spl1 = new char[1] { ',' };
				string[] DMS = stringDMS.Split(spl1, 3);

				char[] spl2 = new char[1] { '/' };
				string[] stringD = DMS[0].Split(spl2, 2);
				double D0 = double.Parse((stringD[0]));
				double D1 = double.Parse(stringD[1]);
				double FloatD = D0 / D1;

				string[] stringM = DMS[1].Split(spl2, 2);
				double M0 = double.Parse(stringM[0]);
				double M1 = double.Parse(stringM[1]);
				double FloatM = M0 / M1;

				string[] stringS = DMS[2].Split(spl2, 2);
				double S0 = double.Parse(stringS[0]);
				double S1 = double.Parse(stringS[1]);
				double FloatS = S0 / S1;

				return (float)(FloatD + (FloatM / 60) + (FloatS / 3600));
			}
		}

		public override void OnActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if ((requestCode == C_REQUEST_PHOTO) && (resultCode == -1)) {
				//var trans = MainDatabase.BeginTransaction();
				var photo = new Photo(); //MainDatabase.CreatePhoto();
				photo.Stamp = DateTimeOffset.Now;
				photo.PhotoPath = file.ToString();
				var photoType = PhotoTypes[PhotoType.SelectedItemPosition];
				photo.PhotoType = photoType.uuid;

				if (photoType.isNeedBrand) {
					var brand = Brands[Brand.SelectedItemPosition];
					photo.Drug = brand.uuid;
				}

				//Latitude and Longitudee
				var exif = new ExifInterface(photo.PhotoPath);
				float[] latLong = new float[2];
				if (exif.GetLatLong(latLong)) {
					photo.Latitude = latLong[0];
					photo.Longitude = latLong[1];
				}

				Photos.Add(photo);

				//trans.Commit();

				RefreshPhotoList();
			}

			// Dispose of the Java side bitmap.
			GC.Collect();
		}

		public override void OnResume()
		{
			base.OnResume();
			if (Pharmacy == null) {
				new Android.App.AlertDialog.Builder(Context)
								   .SetTitle(Resource.String.error_caption)
								   .SetMessage("Отсутствует аптека!")
								   .SetCancelable(false)
								   .SetPositiveButton(@"OK", (dialog, args) => {
									   if (dialog is Android.App.Dialog) {
										   ((Android.App.Dialog)dialog).Dismiss();
									   }
								   })
								   .Show();

			} else if (AttendanceStart == null) {
				Locker.Visibility = ViewStates.Visible;
			}
		}

		public void OnAttendanceStart(DateTimeOffset? start)
		{
			AttendanceStart = start;
			Locker.Visibility = ViewStates.Gone;
			AddPhoto.Enabled = true;
		}

		public void OnAttendanceStop(Transaction openedTransaction, Attendance current)
		{
			if (openedTransaction == null) {
				throw new ArgumentNullException(nameof(openedTransaction));
			}

			if (current == null) {
				throw new ArgumentNullException(nameof(current));
			}

			foreach (var photo in Photos) {
				photo.Attendance = current.UUID;
				MainDatabase.SavePhoto(photo);
			}
		}
	}
}



