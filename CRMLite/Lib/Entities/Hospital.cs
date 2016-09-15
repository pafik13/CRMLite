using System;

using Realms;

namespace CRMLite.Entities
{
	/// <summary>
	/// ЛПУ - Лечебно-профилактические учреждение.
	/// </summary>
	public class Hospital: RealmObject, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор ЛПУ. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		/// <summary>
		/// ФИО работника аптеки.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Адрес аптеки.
		/// </summary>
		/// <value>The address.</value>
		public string Address { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public SyncResult LastSyncResult { get; set; }
	}
}

