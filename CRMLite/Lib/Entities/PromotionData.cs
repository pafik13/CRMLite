﻿using System;
using Realms;

namespace CRMLite.Entities
{
	public class PromotionData : RealmObject, IAttendanceData, IEntity, ISync
	{
		/// <summary>
		/// Уникальный идентификатор акции. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		public string Promotion { get; set; }

		public string Text { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public SyncResult SyncResult { get; set; }

		public bool IsSynced { get; set; }
	}
}
