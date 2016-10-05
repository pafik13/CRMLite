using Java.IO;

namespace CRMLite
{
	public class MaterialItem
	{
		readonly public string MaterialName;
		readonly public File FilePath;

		public MaterialItem(string materialName, File filePath)
		{
			MaterialName = materialName;
			FilePath = filePath;
		}
	}
}

