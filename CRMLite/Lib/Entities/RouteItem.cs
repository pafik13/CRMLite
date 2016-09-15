using System;
using Realms;

namespace CRMLite.Entities
{
	public class RouteItem : RealmObject, IPharmacyData, IEntity
	{
		[PrimaryKey]
		public string UUID { get; set; }

		[Indexed]
		public string Pharmacy { get; set; }

		public DateTimeOffset Date { get; set; }

		public int Order { get; set; }
	}
}

