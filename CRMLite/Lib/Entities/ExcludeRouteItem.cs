using System;
using Realms;

namespace CRMLite.Entities
{
	public class ExcludeRouteItem : RealmObject, IEntity, ISync
	{
		public string UUID { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
	}
}

