using System;
using Realms;

namespace CRMLite.Entities
{
	/// <summary>
	/// Статус аптеки.
	/// </summary>
	public enum PharmacyState
	{
		psActive, psReserve, psClose
	}

	/// <summary>
	/// Аптека.
	/// </summary>
	public class Pharmacy : RealmObject, IEntity
	{
		/// <summary>
		/// Уникальный идентификатор аптеки. Используется Guid.
		/// </summary>
		/// <value>The /.</value>
		[ObjectId]
		public string UUID { get; set; }

		/// <summary>
		/// Отметка: активно/резерв + комментарий/закрыто. Необходимо использовать SetState и GetState.
		/// </summary>
		/// <value>The state.</value>
		public string State { get; set; }

		public void SetState(PharmacyState newState) { State = newState.ToString("G"); }

		public PharmacyState GetState() { return (PharmacyState)Enum.Parse(typeof(PharmacyState), State, true); }

		/// <summary>
		/// Бренд.
		/// </summary>
		/// <value>The brand.</value>
		public string Brand { get; set; }

		/// <summary>
		/// Номер аптеки.
		/// </summary>
		/// <value>The number name.</value>
		public string NumberName { get; set; }

		/// <summary>
		/// Юридическое название аптеки.
		/// </summary>
		/// <value>The legal name.</value>
		public string LegalName { get; set; }

		public string GetName() { return string.Format("{0}, {1}", Brand, Address); }

		/// <summary>
		/// Ссылка на аптечную сеть. UUID класса Net.
		/// </summary>
		/// <value>The net ref.</value>
		[Indexed]
		public string Net { get; set; }

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

		//TODO: добавить поддержку нескольких телефонов
		/// <summary>
		/// Телефон аптеки. 
		/// </summary>
		/// <value>The region.</value>
		public string Phone { get; set; }

		/// <summary>
		/// Где находится. 
		/// </summary>
		/// <value>The place.</value>
		public string Place { get; set; }

		/// <summary>
		/// Категория по данным АС.
		/// </summary>
		/// <value>The сategory.</value>
		public string Category { get; set; }

		/// <summary>
		/// Товарооборот.
		/// </summary>
		/// <value>The turnover.</value>
		public int? TurnOver { get; set; }

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

		/// <summary>
		/// Комментарий по аптеке.
		/// </summary>
		/// <value>The comment.</value>
		public string Comment { get; set; }

		/// <summary>
		/// Дата заведения аптеки. Присваивается при сохранении.
		/// </summary>
		/// <value>The created date.</value>
		public DateTimeOffset? CreatedAt { get; set; }

		/// <summary>
		/// Последний результат синхронизации.
		/// </summary>
		/// <value>The result of last sync.</value>
		public SyncResult LastSyncResult { get; set; }

		public string Email { get; set; }
	}
}
