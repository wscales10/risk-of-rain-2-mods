using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Spotify.Authorisation
{
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	internal class AccessTokenInfo
	{
		public string AccessToken { get; set; }

		public string TokenType { get; set; }

		public string Scope { get; set; }

		public int ExpiresIn { get; set; }
	}
}