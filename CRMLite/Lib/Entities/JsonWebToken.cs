using System;
namespace CRMLite.Entities
{
	public class JsonWebToken
	{
		public string token { get; set; }
		public string owner { get; set; }
		public bool revoked { get; set; }
		public DateTime createdAt { get; set; }
		public DateTime updatedAt { get; set; }
		public string id { get; set; }
	}
}

