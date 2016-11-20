using System;
using Realms;

namespace CRMLite.Entities
{
	public class FinanceDataByMonth: RealmObject, IEntity, IPharmacyData, ISync
	{
		/// <summary>
		/// Уникальный идентификатор информации о продажах. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		[Indexed]
		public string Pharmacy { get; set; }

		public string DrugSKU { get; set; }

		public int Year { get; set; }

		public int Month { get; set; }

		public float? Sale { get; set; }

		public float? Purchase { get; set; }

		public float? Remain { get; set; }

		public string CreatedBy {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public DateTimeOffset CreatedAt {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public DateTimeOffset UpdatedAt {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public bool IsSynced {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}
	}
}

