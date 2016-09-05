using System;
using SD = System.Diagnostics;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Widget;

using RestSharp;

using CRMLite.Entities;

namespace CRMLite
{
	[Activity(Label = "SyncActivity")]
	public class SyncActivity : Activity
	{
		Button GetData;
		Button CheckAll;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.Sync);

			GetData = FindViewById<Button>(Resource.Id.saGetDataB);

			GetData.Click += GetData_Click;

			CheckAll = FindViewById<Button>(Resource.Id.saCheckAll);

			CheckAll.Click += CheckAll_Click;
		}

		void CheckAll_Click(object sender, EventArgs e)
		{
			var mainLL = FindViewById<LinearLayout>(Resource.Id.saMainLL);
			for (int c = 0; c < mainLL.ChildCount; c++) {
				var view = mainLL.GetChildAt(c);
				if (view is CheckBox) {
					((CheckBox)view).Checked = true;
				}
			}
		}

		void GetData_Click(object sender, EventArgs e)
		{
			var client = new RestClient(@"http://front-sblcrm.rhcloud.com/");
			//var client = new RestClient(@"http://sbl-crm-project-pafik13.c9users.io:8080/");

			if (FindViewById<CheckBox>(Resource.Id.saLoadPositionsCB).Checked) LoadPositions(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadNetsCB).Checked) LoadNets(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadSubwaysCB).Checked) LoadSubways(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadRegionsCB).Checked) LoadRegions(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadPlacesCB).Checked) LoadPlaces(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadCategoriesCB).Checked) LoadCategories(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadDrugSKUsCB).Checked) LoadDrugSKUs(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadDrugBrandsCB).Checked) LoadDrugBrands(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadPromotionsCB).Checked) LoadPromotions(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadMessageTypesCB).Checked) LoadMessageTypes(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadPhotoTypesCB).Checked) LoadPhotoTypes(client);
			if (FindViewById<CheckBox>(Resource.Id.saContractsCB).Checked) LoadContracts(client);
			if (FindViewById<CheckBox>(Resource.Id.saWorkTypesCB).Checked) LoadWorkTypes(client);

		}

		void LoadWorkTypes(RestClient client)
		{
			var request = new RestRequest(@"WorkType?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<WorkType>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format(@"Получено WorkType {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadContracts(RestClient client)
		{
			var request = new RestRequest(@"Contract?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<Contract>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format(@"Получено Contract {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadPhotoTypes(RestClient client)
		{
			var request = new RestRequest(@"PhotoType?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<PhotoType>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format(@"Получено PhotoType {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadMessageTypes(RestClient client)
		{
			var request = new RestRequest(@"MessageType?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<MessageType>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				SD.Debug.WriteLine(string.Format(@"Получено MessageType {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}	
		}

		void LoadPromotions(RestClient client)
		{
			var request = new RestRequest(@"Promotion?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<Promotion>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				SD.Debug.WriteLine(string.Format(@"Получено Promotion {0}", response.Data.Count));
				MainDatabase.SaveItems(response.Data);
			}
		}

		void LoadDrugSKUs(RestClient client)
		{
			var request = new RestRequest(@"DrugSKU?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<DrugSKU>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveDrugSKUs(response.Data);
			}
		}

		void LoadDrugBrands(RestClient client)
		{
			var request = new RestRequest(@"DrugBrand?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<DrugBrand>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveDrugBrands(response.Data);
			}		
		}

		void LoadPositions(RestClient client)
		{
			var request = new RestRequest(@"Position?limit=300", Method.GET);
			var response = client.Execute<List<Position>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SavePositions(response.Data);
			}
		}

		void LoadNets(RestClient client)
		{
			var request = new RestRequest(@"Net?limit=300", Method.GET);
			var response = client.Execute<List<Net>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveNets(response.Data);
			}
		}

		void LoadSubways(RestClient client)
		{
			var request = new RestRequest(@"Subway?limit=300", Method.GET);
			var response = client.Execute<List<Subway>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveSubways(response.Data);
			}
		}

		void LoadRegions(RestClient client)
		{
			var request = new RestRequest(@"Region?limit=300", Method.GET);
			var response = client.Execute<List<Region>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveRegions(response.Data);
			}
		}

		void LoadPlaces(RestClient client)
		{
			var request = new RestRequest(@"Place?limit=300", Method.GET);
			var response = client.Execute<List<Place>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SavePlaces(response.Data);
			}
		}

		void LoadCategories(RestClient client)
		{
			var request = new RestRequest(@"Category?limit=300", Method.GET);
			var response = client.Execute<List<Category>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				SD.Debug.WriteLine(response.Data.Count);
				MainDatabase.SaveCategories(response.Data);
			}
		}
	}
}

