using System;
using Realms;

namespace CRMLite
{
	public class ContractData : RealmObject, IEntity, IPharmacyData
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
	}
}

