using System;
using Realms;

namespace CRMLite
{
	public class RouteItem: RealmObject, IPharmacyData
	{
		[Indexed]
		public string Pharmacy { get; set; }

		public DateTimeOffset Date { get; set; }

		public int Order { get; set; }
	}
}

