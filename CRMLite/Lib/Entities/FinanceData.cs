﻿using System;
using Realms;

namespace CRMLite
{
	public class FinanceData : RealmObject, IEntity, IPharmacyData
	{
		/// <summary>
		/// Уникальный идентификатор информации о продажах. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		[Indexed]
		public string Pharmacy { get; set; }

		public DateTimeOffset Period { get; set; }

		public string DrugSKU { get; set; }

		public float? Sale { get; set; }

		public float? Purchase { get; set; }

		public float? Remain { get; set; }
	}
}

