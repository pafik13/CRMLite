using Android.App;
using Android.OS;
using Android.Widget;

namespace CRMLite
{
	[Activity(Label = "MaterialActivity")]
	public class MaterialActivity : Activity
	{
		public const string C_MATERIAL_UUID = @"C_MATERIAL_UUID";

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.Material);

			var materialUUID = Intent.GetStringExtra(C_MATERIAL_UUID);

			if (string.IsNullOrEmpty(materialUUID)) return;

			var material = MainDatabase.GetItem<Material>(materialUUID);

			FindViewById<TextView>(Resource.Id.mMaterialTV).Text = material.name;
		}
	}
}

