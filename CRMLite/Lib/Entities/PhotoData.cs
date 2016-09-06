using System;
using Realms;

namespace CRMLite
{
	public class PhotoData : RealmObject, IAttendanceData, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор фотографии. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		public DateTimeOffset Stamp { get; set; }

		public string Attendance { get; set; }

		public string PhotoType { get; set; }

		public string Brand { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public string PhotoPath { get; set; }
	}
}