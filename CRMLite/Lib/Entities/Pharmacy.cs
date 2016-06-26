using System;
using Realms;

namespace CRMLite.Entities
{
	/// <summary>
	/// Аптека.
	/// </summary>
	public class Pharmacy : RealmObject
	{
		/// <summary>
		/// Уникальный идентификатор аптеки. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		/// <summary>
		/// Обычное название аптеки.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Юридическое название аптеки.
		/// </summary>
		/// <value>The legal name.</value>
		public string LegalName { get; set; }

		/// <summary>
		/// Адрес аптеки.
		/// </summary>
		/// <value>The address.</value>
		public string Address { get; set; }

		/// <summary>
		/// Метро, которое расположено рядом с аптекой.
		/// </summary>
		/// <value>The subway.</value>
		public string Subway { get; set; }

		/// <summary>
		/// Район, в котором расположена аптека.
		/// </summary>
		/// <value>The region.</value>
		public string Region { get; set; }

		/// <summary>
		/// Категория аптеки.
		/// </summary>
		/// <value>The сategory.</value>
		public string Сategory { get; set; }

		/// <summary>
		/// Ссылка на аптечную сеть. UUID класса Net.
		/// </summary>
		/// <value>The net.</value>
		[Indexed]
		public string Net { get; set; }

		/// <summary>
		/// Ссылка на последний визит в аптеку. UUID класса Attendance.
		/// </summary>
		/// <value>The last attendance.</value>
		[Indexed]
		public string LastAttendance { get; set; }

		/// <summary>
		/// Дата последнего визита. Сохраняется значение Attendance.Date. Необходимо для сортировки.
		/// </summary>
		/// <value>The last attendance date.</value>
		public DateTimeOffset? LastAttendanceDate { get; set; }

		/// <summary>
		/// Дата последующего визита. Вычисляется как LastAttendanceDate + Project.DaysToNextAttendance.
		/// </summary>
		/// <value>The next attendance date.</value>
		public DateTimeOffset? NextAttendanceDate { get; set; }

		public DateTimeOffset? CreatedAt { get; set; }

		public SyncResult LastSyncResult { get; set; }

		public string Phone { get; set; }

		public string Email { get; set; }
	}

	/// <summary>
	/// Работник аптеки.
	/// </summary>
	public class Employee : RealmObject
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
		/// Пол работника аптеки.
		/// </summary>
		/// <value>The sex.</value>
		//public Sex Sex { get; set; }

		/// <summary>
		/// Должность работника аптеки.
		/// </summary>
		/// <value>The position.</value>
		public string Position { get; set; }

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
		/// Лояльность работника аптеки.
		/// </summary>
		/// <value>The loyalty.</value>
		public string Loyalty { get; set; }

		/// <summary>
		/// Является ли заказчиком работник аптеки?
		/// </summary>
		/// <value>Is customer?</value>
		public bool IsCustomer { get; set; }

		public DateTimeOffset? CreatedAt { get; set; }
	}

	/// <summary>
	/// Аптечная сеть.
	/// </summary>
	public class Net : RealmObject
	{
		/// <summary>
		/// Уникальный идентификатор аптечной сети. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		/// <summary>
		/// Название аптечной сети.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Юридическое название аптечной сети.
		/// </summary>
		/// <value>The legal name.</value>
		public string LegalName { get; set; }

		/// <summary>
		/// Описание аптечной сети.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }
	}
}

