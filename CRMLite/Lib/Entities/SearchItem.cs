using System;
namespace CRMLite
{
	public class SearchItem
	{
		public string UUID { private set; get; }
		public string Name { private set; get; }
		public string Subway { private set; get; }
		public string Region { private set; get; }
		public string Match { set; get; }

		public SearchItem(string uuid, string name, string subway, string region)
		{
			UUID = uuid;
			Name = name;
			Subway = subway;
			Region = region;
		}
	}
}

