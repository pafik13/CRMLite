﻿using Realms;

namespace CRMLite.Entities
{
	public class Distribution : RealmObject
	{
		/// <summary>
		/// Уникальный идентификатор работника аптеки. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		public string DrugSKU { get; set; }

		public bool IsExistence { get; set; }

		public float Count { get; set; }

		public float Price { get; set; }

		public bool IsPresence { get; set; }

		public string Order { get; set; }

		public string Comment { get; set; }
	}
}

