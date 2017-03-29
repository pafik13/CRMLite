using System;
using Realms;

namespace CRMLite.Entities
{
	public class PhotoData : RealmObject, IAttendanceData, IEntity, ISync
	{
		/// <summary>
		/// Уникальный идентификатор фотографии. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public DateTimeOffset Stamp { get; set; }

		public string Attendance { get; set; }

		public string PhotoType { get; set; }

		public string Brand { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public string PhotoPath { get; set; }

		#region S3
        public string ETag  { get; set; }

		public string Location { get; set; }

		public string Key { get; set; }

		public string Bucket { get; set; }
		#endregion

		#region ISync
		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public SyncResult SyncResult { get; set; }

		public bool IsSynced { get; set; }
		#endregion
	}
}