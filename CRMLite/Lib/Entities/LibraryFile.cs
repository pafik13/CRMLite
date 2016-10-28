using System;

using Realms;

namespace CRMLite.Entities
{
	public class LibraryFile : RealmObject, IEntiryFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string description { get; set; }

		public string fullPath { get; set; }

		public string fileName { get; set; }

		public string s3ETag { get; set; }

		public string s3Location { get; set; }

		public string s3Key { get; set; }

		public string s3Bucket { get; set; }
	}
}

