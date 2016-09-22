using System;
using Realms;

namespace CRMLite.Entities
{
	public class ContractData : RealmObject, IEntity, IPharmacyData, ISync
	{
		/// <summary>
		/// Уникальный идентификатор контракта. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		/// <summary>
		/// Ссылка на аптеку. UUID класса Pharmacy.
		/// </summary>
		/// <value>The pharmacy.</value>
		[Indexed]
		public string Pharmacy { get; set; }

		/// <summary>
		/// Ссылка на контракт. UUID класса Contract.
		/// </summary>
		/// <value>The pharmacy.</value>
		[Indexed]
		public string Contract { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public SyncResult SyncResult { get; set; }

		public bool IsSynced { get; set; }
	}
}

