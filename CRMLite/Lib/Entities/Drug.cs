using Realms;

namespace RealmAndroid.Entities
{
	/// <summary>
	/// Торговая марка лекарства (брэнд, общее наименование).
	/// </summary>
	public class DrugBrand : RealmObject
    {
		/// <summary>
		/// Уникальный идентификатор ТМ лекарства. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		public string UUID { get; set; }

		/// <summary>
		/// Название ТМ лекарства.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Описание ТМ лекарства.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }
    }

	/// <summary>
	/// SKU лекарства (единица хранения).
	/// </summary>
	public class DrugSKU : RealmObject
	{
		/// <summary>
		/// Уникальный идентификатор SKU лекарства. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		public string UUID { get; set; }

		/// <summary>
		/// Название SKU.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Описание SKU.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }

		/// <summary>
		/// Ссылка на ТМ лекарства. UUID класса DrugBrand.
		/// </summary>
		/// <value>The territory.</value>
		[Indexed]
		public string DrugBrand { get; set; }
	}
}
