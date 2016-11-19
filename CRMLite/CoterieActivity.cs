using System.Linq;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content.PM;

using CRMLite.Adapters;
using CRMLite.Entities;

namespace CRMLite
{
	[Activity(Label = "CoterieActivity", ScreenOrientation = ScreenOrientation.Landscape)]
	public class CoterieActivity : Activity
	{
		ListView CoterieTable;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);

			// Create your application here

			SetContentView(Resource.Layout.Coterie);

			FindViewById<Button>(Resource.Id.caCloseB).Click += (sender, e) => {
				Finish();
			};

			CoterieTable = FindViewById<ListView>(Resource.Id.caCoterieTable);
			var header = LayoutInflater.Inflate(Resource.Layout.CoterieTableHeader, CoterieTable, false);
			CoterieTable.AddHeaderView(header);
		}

		protected override void OnResume()
		{
			base.OnResume();

			var attendances = MainDatabase.GetItems<CoterieData>()
										  .GroupBy(cd => cd.Attendance)
										  .Select(g => MainDatabase.GetEntity<Attendance>(g.Key))
										  .OrderBy(a => a.When)
			                              .ToList();
			
			CoterieTable.Adapter = new CoterieAdapter(this, attendances);
		}
	}
}

