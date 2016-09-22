﻿using System;
using System.Collections.Generic;
using Realms;

namespace CRMLite.Entities
{
	public class CoterieData: RealmObject, IAttendanceData, IEntity, ISync
	{
		/// <summary>
		/// Уникальный идентификатор фарм-кружка. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		public string Employee { get; set; }

		public string Brand { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public SyncResult SyncResult { get; set; }

		public bool IsSynced { get; set; }
	}

	public class CoterieDataGrouped
	{
		public Attendance Attendance { get; set; }

		public Dictionary<string, Employee> Employees { get; }

		public Dictionary<string, DrugBrand> Brands { get; }

		public CoterieDataGrouped(Attendance attendance)
		{
			Attendance = attendance;
			Employees = new Dictionary<string, Employee>();
			Brands = new Dictionary<string, DrugBrand>();
		}
	}
}
