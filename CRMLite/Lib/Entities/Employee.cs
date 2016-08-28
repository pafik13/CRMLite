using System;
using Realms;

namespace CRMLite.Entities
{
	/// <summary>
	/// Статус аптеки.
	/// </summary>
	public enum Sex
	{
		Male, Female
	}

	/// <summary>
	/// Работник аптеки.
	/// </summary>
	public class Employee : RealmObject, IEntity, IPharmacyData
	{
		/// <summary>
		/// Уникальный идентификатор работника аптеки. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		/// <summary>
		/// Ссылка на аптеку. UUID класса Pharmacy.
		/// </summary>
		/// <value>The pharmacy.</value>
		[Indexed]
		public string Pharmacy { get; set; }

		/// <summary>
		/// ФИО работника аптеки.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Пол работника аптеки. Необходимо использовать SetSex и GetSex.
		/// </summary>
		/// <value>The sex.</value>
		public string Sex { get; set; }

		public void SetSex(Sex newSex) { Sex = newSex.ToString("G"); }

		public Sex GetSex() { return (Sex)Enum.Parse(typeof(Sex), Sex, true); }

		/// <summary>
		/// Должность работника аптеки.
		/// </summary>
		/// <value>The position.</value>
		public string Position { get; set; }

		/// <summary>
		/// Является ли заказчиком работник аптеки?
		/// </summary>
		/// <value>Is customer?</value>
		public bool IsCustomer { get; set; }

		/// <summary>
		/// Дата рождения работника аптеки.
		/// </summary>
		/// <value>The birth date.</value>
		public DateTimeOffset? BirthDate { get; set; }

		/// <summary>
		/// Контактный телефон работника аптеки.
		/// </summary>
		/// <value>The phone.</value>
		public string Phone { get; set; }

		/// <summary>
		/// Контактный e-mail работника аптеки.
		/// </summary>
		/// <value>The e-mail.</value>
		public string Email { get; set; }

		/// <summary>
		/// Возможность участия в конференциях.
		/// </summary>
		/// <value>Can participate conference?</value>
		public bool CanParticipate { get; set; }

		/// <summary>
		/// Комментарий по сотруднику.
		/// </summary>
		/// <value>The comment.</value>
		public string Comment { get; set; }

		/// <summary>
		/// Дата заведения сотрудника. Присваивается при сохранении.
		/// </summary>
		/// <value>The created date.</value>
		public DateTimeOffset? CreatedAt { get; set; }

		/// <summary>
		/// Последний результат синхронизации.
		/// </summary>
		/// <value>The result of last sync.</value>
		public SyncResult LastSyncResult { get; set; }
	}
}

