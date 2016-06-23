using Realms;
using RestSharp.Serializers;


namespace RealmAndroid.Entities
{
	public enum Sex
	{
		Male, Female
	}

	// Define your models like regular C# classes
	public class Dog : RealmObject
	{
		/// <summary>
		/// Первичный ключ. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[ObjectId]
		public string UUID { get; set; }

		/// <summary>
		/// Ссылка на владельца. Хранится UUID класса Person.
		/// </summary>
		/// <value>The owner.</value>
		[Indexed]
		public string Owner { get; set; }


		[Ignored]
		public double? Latitude { get; set; }


		public string Name { get; set; }


		public int Age { get; set; }
	}

	public class Person : RealmObject
	{
		[ObjectId]
		public string UUID { get; set; }

		public string Name { get; set; }
	}

}

