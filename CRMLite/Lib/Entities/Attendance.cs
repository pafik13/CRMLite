using System;
using Realms;

namespace CRMLite
{
	public class Attendance : RealmObject
	{
		/// <summary>
		/// Уникальный идентификатор работника аптеки. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		public DateTimeOffset When { get; set; }
	}
}

