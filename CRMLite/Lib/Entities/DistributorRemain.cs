using System;
using Realms;

namespace CRMLite.Entities
{
	public class DistributorRemain : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string distributor { get; set; }

		public DateTimeOffset date { get; set; }

		public int remain { get; set; }
	}
}

