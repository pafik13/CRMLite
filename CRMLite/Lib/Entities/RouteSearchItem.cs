namespace CRMLite
{
	public class RouteSearchItem
	{
		public string UUID { private set; get; }
		public string Name { private set; get; }
		public string Subway { private set; get; }
		public string Region { private set; get; }
		public string Brand { private set; get; }
		public string Address { private set; get; }
		public string Match { set; get; }
		public bool IsVisible { set; get; }

		public RouteSearchItem(string uuid, string name, string subway, string region, string brand, string address)
		{
			UUID = uuid;
			Name = name;
			Subway = subway;
			Region = region;
			Brand = brand;
			Address = address;
			IsVisible = true;
		}
	}
}

