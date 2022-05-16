using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SpotifyAPI.Web;

namespace Spotify.Authorisation
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    internal class RefreshTokenInfo : AccessTokenInfo
    {
        public string RefreshToken { get; set; }
    }
}