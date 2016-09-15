using Realms;

namespace CRMLite.Entities
{
	public class Message : RealmObject, IEntity
	{
		[PrimaryKey]
		public string UUID { get; set; }

		public string Text { get; set; }
	}
}