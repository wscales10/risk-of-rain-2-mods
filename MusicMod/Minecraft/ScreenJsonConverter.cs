using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Minecraft
{
	public class ScreenJsonConverter : JsonConverter
	{
		public override bool CanWrite => false;

		public override bool CanRead => true;

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IScreen);
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new InvalidOperationException("Use default serialization.");
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var jsonObject = JObject.Load(reader);
			Screen? screen = new();
			serializer.Populate(jsonObject.CreateReader(), screen);
			return screen;
		}
	}
}