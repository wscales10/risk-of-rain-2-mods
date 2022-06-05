using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Spotify.Authorisation
{
    public delegate Task<HttpResponseMessage> Sender(HttpRequestMessage request);

    internal class CodeFlow : CodeFlowBase
    {
        public CodeFlow(App app) : base(app)
        {
        }

        internal event Func<HttpClient> ClientRequested;

        private string serverName
        {
            get
            {
                return "http://localhost:7071";

                // return "http://WoodyScales.Auth.Spotify.azurewebsites.net";
            }
        }

        protected override object GetClient() => ClientRequested?.Invoke();

        protected override async Task<object> refreshTokenAsync(object client) => await ((HttpClient)client).SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{serverName}/api/refresh-tokens?refreshToken={RefreshToken}"));

        protected override async Task<object> requestTokensAsync(object client) => await ((HttpClient)client).SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{serverName}/api/request-tokens?code={Code}"));
    }
}