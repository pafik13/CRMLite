using System;
using Realms;

namespace CRMLite.Entities
{
	public class SyncItem
	{
		public SyncItem()
		{
			CreatedAt = DateTimeOffset.Now;
			IsSynced = false;
		}

		public string ObjectUUID { get; set; }

		public string Path { get; set;}

		public string JSON { get; set; }

		public DateTimeOffset CreatedAt { get; }

		public DateTimeOffset TrySyncAt { get; set; }

		public bool IsSynced { get; set; }

		//DateTimeOffset SyncedAt { get; set; }
	}

	public class SyncResult: RealmObject
	{
		public int id { get; set;}

		public DateTimeOffset createdAt { get; set; }

		public DateTimeOffset updatedAt { get; set; }
	}
}

