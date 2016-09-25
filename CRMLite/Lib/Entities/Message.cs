using System;
using Realms;

namespace CRMLite.Entities
{
	public class Message : RealmObject, IEntity, ISync
	{
		[PrimaryKey]
		public string UUID { get; set; }

		public string Text { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public SyncResult SyncResult { get; set; }

		public bool IsSynced { get; set; }
	}
}