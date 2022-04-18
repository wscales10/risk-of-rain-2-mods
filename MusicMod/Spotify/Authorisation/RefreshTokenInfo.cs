using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Spotify.Authorisation
{
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	internal class RefreshTokenInfo : AccessTokenInfo
	{
		public string RefreshToken { get; set; }
	}
}
