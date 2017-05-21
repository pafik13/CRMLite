using System;
using Realms;

namespace CRMLite.Entities
{
	public enum MaterialType
	{
		mtNone, mtDoctorsOnly, mtPharmaciesOnly, mtBoth
	}

	/// <summary>
	/// Представитель/сотрудник.
	/// </summary>
	public class Agent : RealmObject, IEntiryFromServer
    {
		/// <summary>
		/// Уникальный идентификатор представителя/сотрудника. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		public string uuid { get; set; }

		/// <summary>
		/// Имя представителя/сотрудника.
		/// </summary>
		/// <value>The first name.</value>
		public string firstName { get; set; }

		/// <summary>
		/// Отчество представителя/сотрудника.
		/// </summary>
		/// <value>The name of the middle.</value>
		public string middleName { get; set; }

		/// <summary>
		/// Фамилия представителя/сотрудника.
		/// </summary>
		/// <value>The last name.</value>
		public string lastName { get; set; }

		public string shortName { get; set; }
		public string fullName { get; set; }

		/// <summary>
		/// Пол представителя/сотрудника.
		/// </summary>
		/// <value>The sex.</value>
		public string sex { get; set; }

		public void SetSex(Sex newSex) { sex = newSex.ToString("G"); }

		public Sex GetSex() { return (Sex)Enum.Parse(typeof(Sex), sex, true); }

		/// <summary>
		/// Дата приема на работу представителя/сотрудника.
		/// </summary>
		/// <value>The hire date.</value>
		public DateTimeOffset hireDate { get; set; }

		/// <summary>
		/// Дата рождения представителя/сотрудника.
		/// </summary>
		/// <value>The birth date.</value>
		public DateTimeOffset birthDate { get; set; }

		/// <summary>
		/// Почтовый адрес представителя/сотрудника.
		/// </summary>
		/// <value>The post address.</value>
		public string postAddress { get; set; }

		/// <summary>
		/// Контактный телефон представителя/сотрудника.
		/// </summary>
		/// <value>The phone.</value>
		public string phone { get; set; }

		/// <summary>
		/// Электронная почта представителя/сотрудника.
		/// </summary>
		public string email { get; set; }

		/// <summary>
		/// Город работы.
		/// </summary>
		public string city { get; set; }

		/// <summary>
		/// Количество недель в маршруте.
		/// </summary>
		public int weeksInRout { get; set; }

		/// <summary>
		/// Режим работ.
		/// </summary>
		public string workMode { get; set; }

		public void SetWorkMode(WorkMode newWorkMode) { workMode = newWorkMode.ToString("G"); }

		public WorkMode GetWorkMode() { return (WorkMode)Enum.Parse(typeof(WorkMode), workMode, true); }

		/// <summary>
		/// User.
		/// </summary>
    	public string user { get; set; }

		/// <summary>
		/// Тип использования материалов.
		/// </summary>
		/// <value>The sex.</value>
		public string materialType { get; set; }

		public void SetMaterialType(Sex newMaterialType) { materialType = newMaterialType.ToString("G"); }

		public MaterialType GetMaterialType() { return (MaterialType)Enum.Parse(typeof(MaterialType), materialType, true); }
	}
}
