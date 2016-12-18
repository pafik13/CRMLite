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
		public const string C_ATTENDANCE_LAST_UUID = @"C_ATTENDANCE_LAST_UUID";

		public const int C_REQUEST_PHOTO = 100;

		LayoutInflater Inflater;

		Pharmacy Pharmacy;

		public Dictionary<string, bool> Agreements { get; private set; }

		DateTimeOffset? AttendanceStart;
		TextView Locker;
		ImageView Arrow;

		IList<PhotoType> PhotoTypes;
		Spinner PhotoType;
		IList<DrugBrand> Brands;
		Spinner Brand;
		Button AddPhoto;
		LinearLayout PhotoTable;
		static Java.IO.File File;
		List<PhotoData> Photos;

		internal string GetUndonePhotoTypes()
		{
			string result = string.Empty;

			foreach (var item in Agreements) {
				if (item.Value) continue;

				if (item.Key.Contains(":")) {
					string[] keys = item.Key.Split(new char[] { ':' });
					var photoType = MainDatabase.GetItem<PhotoType>(keys[0]);
					var brand = MainDatabase.GetItem<DrugBrand>(keys[1]);
					result += string.Format("  - тип '{0}' для бренда '{1}'{2}", photoType.name, brand.name, System.Environment.NewLine);
				} else {
					var photoType = MainDatabase.GetItem<PhotoType>(item.Key);
					result += string.Format("  - тип '{0}'{1}", photoType.name, System.Environment.NewLine);
				}
			}

			return result;
		}

		/**
		 * Factory method for this fragment class. Constructs a new fragment for the given page number.
		 */
		public static PhotoFragment create(string pharmacyUUID, string attendanceLastUUID)
		{
			PhotoFragment fragment = new PhotoFragment();
			Bundle arguments = new Bundle();
			arguments.PutString(C_PHARMACY_UUID, pharmacyUUID);
			arguments.PutString(C_ATTENDANCE_LAST_UUID, attendanceLastUUID);
			fragment.Arguments = arguments;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var pharmacyUUID = Arguments.GetString(C_PHARMACY_UUID);
			if (string.IsNullOrEmpty(pharmacyUUID)) return;

			Pharmacy = MainDatabase.GetPharmacy(pharmacyUUID);

			Agreements = new Dictionary<string, bool>();
			string key = string.Empty;
			foreach (var agreement in MainDatabase.GetItems<PhotoAgreement>()
												  .Where(pa => (pa.object_type == "all"))
					) {
				if (string.IsNullOrEmpty(agreement.brand)) {
					key = agreement.photoType;
				} else {
					key = string.Format("{0}:{1}", agreement.photoType, agreement.brand);
				}
				if (!Agreements.ContainsKey(key)) {
					Agreements.Add(key, false);
				}
			}
			foreach (var agreement in MainDatabase.GetItems<PhotoAgreement>()
													  .Where(pa => (pa.object_type == "pharmacy")
																&& (pa.object_uuid == Pharmacy.UUID)
															)
					) {
				if (string.IsNullOrEmpty(agreement.brand)) {
					key = agreement.photoType;
				} else {
					key = string.Format("{0}:{1}", agreement.photoType, agreement.brand);
				}
				if (!Agreements.ContainsKey(key)) {
					Agreements.Add(key, false);
				}
			}
			if (!string.IsNullOrEmpty(Pharmacy.Net)) {
				foreach (var agreement in MainDatabase.GetItems<PhotoAgreement>()
														  .Where(pa => (pa.object_type == "net")
																	&& (pa.object_uuid == Pharmacy.Net)
																)
						) {
					if (string.IsNullOrEmpty(agreement.brand)) {
						key = agreement.photoType;
					} else {
						key = string.Format("{0}:{1}", agreement.photoType, agreement.brand);
					}
					if (!Agreements.ContainsKey(key)) {
						Agreements.Add(key, false);
					}
				}
			}
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			base.OnCreateView(inflater, container, savedInstanceState);

			Inflater = inflater;

			// Inflate the layout containing a title and body text.
			View view = inflater.Inflate(Resource.Layout.PhotoFragment, container, false);

			//if (Photos == null) {
			//	Photos = new List<PhotoData>();
			//}
			var attendanceLastUUID = Arguments.GetString(C_ATTENDANCE_LAST_UUID);
			var attendanceLast = string.IsNullOrEmpty(attendanceLastUUID) ? null : MainDatabase.GetEntity<Attendance>(attendanceLastUUID);

			Photos = (attendanceLast == null) ?
				new List<PhotoData>() : MainDatabase.GetDatas<PhotoData>(attendanceLastUUID) ?? new List<PhotoData>();

			PhotoTable = view.FindViewById<LinearLayout>(Resource.Id.pfPhotoTableLL);
			RefreshPhotoList();


			Brand = view.FindViewById<Spinner>(Resource.Id.pfBrandS);
			Brands = MainDatabase.GetItems<DrugBrand>();

			PhotoType = view.FindViewById<Spinner>(Resource.Id.pfPhotoTypeS);
			PhotoTypes = MainDatabase.GetItems<PhotoType>();


			AddPhoto = view.FindViewById<Button>(Resource.Id.pfAddPhotoB);
			AddPhoto.Click += (object sender, EventArgs e) => {
				//	if (Common.CreateDirForPhotos(user)) {
				//		string type = photoTypes[spnPhotoTypes.SelectedItemPosition].name;
				//		type = Transliteration.Front(type, TransliterationType.Gost).Substring(0, Math.Min(5, type.Length)).ToUpper();
				//		string subtype = currentPhotoSubTypes[spnPhotoSubTypes.SelectedItemPosition].name;
				//		subtype = Transliteration.Front(subtype, TransliterationType.Gost).Substring(0, Math.Min(5, subtype.Length)).ToUpper();
				string stamp = DateTime.Now.ToString(@"yyyyMMddHHmmsszz");
				File = new Java.IO.File(Helper.PhotoDir, string.Format("PHOTO_{0}.jpg", stamp));
				var intent = new Intent(MediaStore.ActionImageCapture);
				intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(File));
				StartActivityForResult(intent, C_REQUEST_PHOTO);
				//	}
			};

			Locker = view.FindViewById<TextView>(Resource.Id.locker);
			Arrow = view.FindViewById<ImageView>(Resource.Id.arrow);

			if (attendanceLast != null) {
				if (attendanceLast.When.Date == DateTimeOffset.UtcNow.Date) {
					Arrow.Visibility = ViewStates.Gone;
					Locker.Text = string.Empty;
				}
			}

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

				string drugBrand = (photoType.isNeedBrand) ? MainDatabase.GetItem<DrugBrand>(item.Brand).name : @"<не требуется>";
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
			}

			char[] spl1 = { ',' };
			string[] DMS = stringDMS.Split(spl1, 3);

			char[] spl2 = { '/' };
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

		public override void OnActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if ((requestCode == C_REQUEST_PHOTO) && (resultCode == -1)) {
				//var trans = MainDatabase.BeginTransaction();
				var photo = new PhotoData(); //MainDatabase.CreatePhoto();
				photo.Stamp = DateTimeOffset.Now;
				photo.PhotoPath = File.ToString();
				var photoType = PhotoTypes[PhotoType.SelectedItemPosition];
				photo.PhotoType = photoType.uuid;

				string key = string.Empty;
				if (photoType.isNeedBrand) {
					photo.Brand = Brands[Brand.SelectedItemPosition - 1].uuid;
					key = string.Format("{0}:{1}", photo.PhotoType, photo.Brand); ;
				} else {
					key = photo.PhotoType;
				}

				if (Agreements.ContainsKey(key)) {
					Agreements[key] = true;
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
			Arrow.Visibility = ViewStates.Gone;
			Locker.Visibility = ViewStates.Gone;

			var brandsList = new List<DrugBrand>();
			brandsList.Add(new DrugBrand { name = @"Выберите бренд!", uuid = Guid.Empty.ToString() });
			brandsList.AddRange(Brands);
			var brandFullAdapter = new ArrayAdapter(
				Activity, Android.Resource.Layout.SimpleSpinnerItem, brandsList.Select(i => i.name).ToArray()
			);
			brandFullAdapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			var brandEmptyAdapter = new ArrayAdapter(
				Activity, Android.Resource.Layout.SimpleSpinnerItem, new string[] { @"<НЕ ТРЕБУЕТСЯ>" }
			);

			PhotoType.Clickable = true;
			var adapter = new ArrayAdapter(
				Activity, Android.Resource.Layout.SimpleSpinnerItem, PhotoTypes.Select(i => i.name).ToArray()
			);
			adapter.SetDropDownViewResource(Resource.Layout.SpinnerItem);
			PhotoType.Adapter = adapter;
			PhotoType.ItemSelected += (sender, e) => {
				if (PhotoTypes[e.Position].isNeedBrand) {
					Brand.Adapter = brandFullAdapter;
					Brand.Clickable = true;
					AddPhoto.Enabled = false;
				} else {
					Brand.Adapter = brandEmptyAdapter;
					Brand.Clickable = false;
					AddPhoto.Enabled = true;
				}
			};
			PhotoType.SetSelection(0);
			Brand.ItemSelected += (sender, e) => {
				if (sender is Spinner) {
					if (((Spinner)sender).Clickable) {
						if (e.Position > 0) {
							AddPhoto.Enabled = true;
						} else {
							AddPhoto.Enabled = false;
						}
					}
				}
			};
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
				var photoData = MainDatabase.CreateData<PhotoData>(current.UUID);
				photoData.PhotoType = photo.PhotoType;
				photoData.Brand = photo.Brand;
				photoData.Stamp = photo.Stamp;
				photoData.PhotoPath = photo.PhotoPath;
				photoData.Latitude = photo.Latitude;
				photoData.Longitude = photo.Longitude;
			}
		}
	}
}

