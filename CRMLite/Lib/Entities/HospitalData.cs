﻿using System;
using Realms;

namespace CRMLite.Entities
{
	public class HospitalData: RealmObject, IEntity, IPharmacyData, ISync
	{
		/// <summary>
		/// Уникальный идентификатор ЛПУ. Используется Guid.
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
		/// Ссылка на поликлинику. UUID класса Hospital.
		/// </summary>
		/// <value>The hospital.</value>
		public string Hospital { get; set; }

		/// <summary>
		/// Ссылка на поликлинику из списка. uuid класса ListedHospital.
		/// </summary>
		/// <value>The listed hospital.</value>
		public string ListedHospital { get; set; }

		public string CreatedBy{ get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public SyncResult SyncResult { get; set; }

		public bool IsSynced { get; set; }
	}
}
