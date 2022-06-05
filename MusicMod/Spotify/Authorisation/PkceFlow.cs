using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace Spotify.Authorisation
{
    internal class PkceFlow : CodeFlowBase
    {
        private readonly string codeVerifier;

        private readonly string codeChallenge;

        public PkceFlow(App app) : base(app)
        {
            (codeVerifier, codeChallenge) = PKCEUtil.GenerateCodes();
        }

        public override NameValueCollection GetLoginQueryString(IEnumerable<string> scopes)
        {
            var output = base.GetLoginQueryString(scopes);
            output.Add("code_challenge_method", "S256");
            output.Add("code_challenge", codeChallenge);
            return output;
        }

        protected override object GetClient() => new OAuthClient();

        protected override async Task<object> requestTokensAsync(object client)
        {
            try
            {
                return await ((OAuthClient)client).RequestToken(new PKCETokenRequest(app.ClientId, Code, app.RedirectUri, codeVerifier));
            }
            catch (APIException)
            {
                ErrorState = ErrorState.ApiDenied;
                return null;
            }
        }

        protected override async Task<object> refreshTokenAsync(object client)
        {
            try
            {
                return await ((OAuthClient)client).RequestToken(new PKCETokenRefreshRequest(app.ClientId, RefreshToken));
            }
            catch (APIException)
            {
                ErrorState = ErrorState.ApiDenied;
                return null;
            }
        }
    }
}