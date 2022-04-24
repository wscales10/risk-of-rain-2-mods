using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Spotify
{
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	public class DeviceInfo
	{
		public string Id { get; set; }

		public bool IsActive { get; set; }

		public bool IsPrivateSession { get; set; }

		public bool IsRestricted { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public int VolumePercent { get; set; }
	}
}