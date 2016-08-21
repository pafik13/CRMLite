﻿using Realms;

namespace CRMLite.Entities
{
	public class CoterieData: RealmObject, IAttendanceData
	{
		/// <summary>
		/// Уникальный идентификатор фарм-кружка. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		public string Employee { get; set; }

		public RealmList<DrugBrand> Brands { get; }
	}
}
