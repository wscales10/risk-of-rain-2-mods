using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace Spotify.Authorisation
{
    internal class PkceFlow : CodeFlow
    {
        private readonly string codeVerifier;

        private readonly string codeChallenge;

        public PkceFlow(App app) : base(app)
        {
            (codeVerifier, codeChallenge) = PKCEUtil.GenerateCodes();
        }

        protected override AuthenticationHeaderValue AuthorisationHeaderValue => null;

        public override NameValueCollection GetLoginQueryString(IEnumerable<string> scopes)
        {
            var output = base.GetLoginQueryString(scopes);
            output.Add("code_challenge_method", "S256");
            output.Add("code_challenge", codeChallenge);
            return output;
        }

        protected override async Task<object> RequestTokensAsync(OAuthClient client) => await client.RequestToken(new PKCETokenRequest(app.ClientId, Code, app.RedirectUri, codeVerifier));

        protected override async Task<object> RefreshTokenAsync(OAuthClient client) => await client.RequestToken(new PKCETokenRefreshRequest(app.ClientId, RefreshToken));
    }
}