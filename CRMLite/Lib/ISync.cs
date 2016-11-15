using System;

namespace CRMLite.Entities
{
	public interface ISync
	{
		/// <summary>
		/// Link to Agent UUID
		/// </summary>
		/// <value>The created by.</value>
		string CreatedBy { get; set; } 
		DateTimeOffset CreatedAt { get; set; }
		DateTimeOffset UpdatedAt { get; set; }
		//SyncResult SyncResult { get; set; }
		bool IsSynced { get; set; }
	}
}

