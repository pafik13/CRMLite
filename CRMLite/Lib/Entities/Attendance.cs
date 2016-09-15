using System;
using Realms;

namespace CRMLite
{
	public class Attendance : RealmObject, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор работника аптеки. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public string Pharmacy { get; set; }

		public DateTimeOffset When { get; set; }

		public int Duration { get; set; }
	}
}

