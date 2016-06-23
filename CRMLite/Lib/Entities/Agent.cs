using System;
using Realms;

namespace RealmAndroid.Entities
{
	/// <summary>
	/// Представитель/сотрудник.
	/// </summary>
	public class Agent : RealmObject
    {
		/// <summary>
		/// Уникальный идентификатор представителя/сотрудника. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		public string UUID { get; set; }

		/// <summary>
		/// Имя представителя/сотрудника.
		/// </summary>
		/// <value>The first name.</value>
		public string FirstName { get; set; }

		/// <summary>
		/// Отчество представителя/сотрудника.
		/// </summary>
		/// <value>The name of the middle.</value>
		public string MiddleName { get; set; }

		/// <summary>
		/// Фамилия представителя/сотрудника.
		/// </summary>
		/// <value>The last name.</value>
		public string LastName { get; set; }

		/// <summary>
		/// Пол представителя/сотрудника.
		/// </summary>
		/// <value>The sex.</value>
		public Sex Sex { get; set; }

		/// <summary>
		/// Дата приема на работу представителя/сотрудника.
		/// </summary>
		/// <value>The hire date.</value>
		public DateTime HireDate { get; set; }

		/// <summary>
		/// Дата рождения представителя/сотрудника.
		/// </summary>
		/// <value>The birth date.</value>
		public DateTime BirthDate { get; set; }

		/// <summary>
		/// Почтовый адрес представителя/сотрудника.
		/// </summary>
		/// <value>The post address.</value>
		public string PostAddress { get; set; }

		/// <summary>
		/// Контактный телефон представителя/сотрудника.
		/// </summary>
		/// <value>The phone.</value>
		public string Phone { get; set; }

		/// <summary>
		/// Ссылка на должность. UUID класса Position.
		/// </summary>
		/// <value>The position.</value>
		[Indexed]
		public string Position { get; set; }

		/// <summary>
		/// Ссылка на менеджера. UUID класса Manager.
		/// </summary>
		/// <value>The manager.</value>
		[Indexed]
        public string Manager { get; set; }

		/// <summary>
		/// Ссылка на проект. UUID класса Project.
		/// </summary>
		/// <value>The project.</value>
		[Indexed]
        public string Project { get; set; }

		/// <summary>
		/// Ссылка на территорию работы. UUID класса Territory.
		/// </summary>
		/// <value>The territory.</value>
		[Indexed]
        public string Territory { get; set; }

		//public int user { get; set; }
		//public string job_role { get; set; }
	}
}
