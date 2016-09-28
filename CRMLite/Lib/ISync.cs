using System;

namespace CRMLite.Entities
{
	public interface ISync
	{
		string CreatedBy { get; set; } // link to Agent UUID
		DateTimeOffset CreatedAt { get; set; }
		DateTimeOffset UpdatedAt { get; set; }
		//SyncResult SyncResult { get; set; }
		bool IsSynced { get; set; }
	}
}

