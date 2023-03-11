using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

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

		public static T FromJson<T>(string json, Action<JsonSerializerSettings> settingsBuilder)
		{
			var settings = new JsonSerializerSettings(defaultSettings);
			settingsBuilder(settings);
			return JsonConvert.DeserializeObject<T>(json, settings);
		}

		public static T FromJson<T>(string json, JsonSerializerSettings settings) => JsonConvert.DeserializeObject<T>(json, settings);

		public static T FromJson<T>(string json, params JsonConverter[] converters) => JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings(defaultSettings) { Converters = converters });

		public static string ToJson(object value) => JsonConvert.SerializeObject(value, defaultSettings);
	}
}