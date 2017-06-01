using Realms;

using CRMLite.Entities;

namespace CRMLite
{
	public class Material: RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }

		public string fileName { get; set; }

		public string s3ETag { get; set; }

		public string s3Location { get; set; }

		public string s3Key { get; set; }

		public string s3Bucket { get; set; }

		/// <summary>
		/// Тип использования материалов.
		/// </summary>
		/// <value>The material type.</value>
		public string type { get; set; }

		public MaterialType Type {
			get { return type.ToEnum(MaterialType.mtNone); }
			set { type = value.ToString("G"); }
		}

		/// <summary>
		/// Получение расположения файла на планшете.
		/// </summary>
		/// <value>Get material local path.</value>
		public string GetLocalPath()
		{
			return GetJavaFile().ToString();
		}

		/// <summary>
		/// Получение java-файл объекта.
		/// </summary>
		/// <value>Get material local path.</value>
		public Java.IO.File GetJavaFile()
		{
			return new Java.IO.File(Helper.MaterialDir, fileName);
		}
	}
}

