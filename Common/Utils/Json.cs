using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Utils
{
	public static class Json
	{
		private static readonly JsonSerializerSettings defaultSettings = new JsonSerializerSettings()
		{
			ContractResolver = new DefaultContractResolver()
			{
				NamingStrategy = new CamelCaseNamingStrategy()
			},

			Formatting = Formatting.Indented
		};

		public static T FromJson<T>(string json) => JsonConvert.DeserializeObject<T>(json, defaultSettings);

		public static string ToJson(object value) => JsonConvert.SerializeObject(value, defaultSettings);
	}
}