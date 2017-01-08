using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp.Serializers;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace CRMLite.Entities
{
	/// <summary>
	/// Default JSON serializer for request bodies
	/// Doesn't currently use the SerializeAs attribute, defers to Newtonsoft's attributes
	/// </summary>
	public class NewtonsoftJsonSerializer : ISerializer
	{
		private readonly JsonSerializer _serializer;

		/// <summary>
		/// Default serializer
		/// </summary>
		public NewtonsoftJsonSerializer()
		{
			ContentType = "application/json";
			_serializer = new JsonSerializer {
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Include,
				DefaultValueHandling = DefaultValueHandling.Include,
				ContractResolver = new ShouldSerializeContractResolver()
			};
		}

		/// <summary>
		/// Default serializer with overload for allowing custom Json.NET settings
		/// </summary>
		public NewtonsoftJsonSerializer(JsonSerializer serializer)
		{
			ContentType = "application/json";
			_serializer = serializer;
		}

		/// <summary>
		/// Serialize the object as JSON
		/// </summary>
		/// <param name="obj">Object to serialize</param>
		/// <returns>JSON as String</returns>
		public string Serialize(object obj)
		{
			using (var stringWriter = new StringWriter()) {
				using (var jsonTextWriter = new JsonTextWriter(stringWriter)) {
					jsonTextWriter.Formatting = Formatting.Indented;
					jsonTextWriter.QuoteChar = '"';

					_serializer.Serialize(jsonTextWriter, obj);

					var result = stringWriter.ToString();
					return result;
				}
			}
		}

		/// <summary>
		/// Unused for JSON Serialization
		/// </summary>
		public string DateFormat { get; set; }
		/// <summary>
		/// Unused for JSON Serialization
		/// </summary>
		public string RootElement { get; set; }
		/// <summary>
		/// Unused for JSON Serialization
		/// </summary>
		public string Namespace { get; set; }
		/// <summary>
		/// Content type for serialized content
		/// </summary>
		public string ContentType { get; set; }
	}

	public class ShouldSerializeContractResolver : DefaultContractResolver
 	{
		public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);

			if (property.DeclaringType == typeof(Realms.RealmObject) && property.PropertyName == "Realm")
			{
				property.ShouldSerialize = (obj) => { return false; };
			}

			if (property.DeclaringType == typeof(Realms.RealmObject) && property.PropertyName == "ObjectSchema") {
				property.ShouldSerialize = (obj) => { return false; };
			}

			return property;
		}
	}
}