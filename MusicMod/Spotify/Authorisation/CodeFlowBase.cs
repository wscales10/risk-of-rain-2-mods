using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using Utils;
using Utils.Reflection.Properties;

namespace Spotify.Authorisation
{
    internal abstract class CodeFlowBase
    {
        protected readonly App app;

        private ErrorState errorState;

        private FlowState state;

        protected CodeFlowBase(App app)
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] stateData = new byte[12];
                rng.GetBytes(stateData);
                StateString = Convert.ToBase64String(stateData);
            }

            this.app = app;
        }

        public string Code { get; private set; }

        public ErrorState ErrorState
        {
            get => errorState;

            protected set
            {
                if ((errorState = value) == ErrorState.None)
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    state = FlowState.Error;
                }

                this.LogPropertyValue(value);
            }
        }

        public string RefreshToken { get; private set; }

        public FlowState State
        {
            get => state;

            protected set
            {
                if ((state = value) == FlowState.Error)
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    errorState = ErrorState.None;
                }

                this.LogPropertyValue(value);
            }
        }

        public string StateString { get; }

        public bool IsFaulted => State == FlowState.Error;

        public virtual NameValueCollection GetLoginQueryString(IEnumerable<string> scopes)
        {
            State = FlowState.Login;
            var output = HttpUtility.ParseQueryString(string.Empty);
            output.Add("response_type", "code");
            output.Add("client_id", app.ClientId);
            output.Add("scope", string.Join(" ", scopes));
            output.Add("redirect_uri", app.RedirectUri.ToString());
            output.Add("state", StateString);
            return output;
        }

        public async Task<RefreshTokenInfo> RequestTokensAsync()
        {
            ThrowIfFaulted();
            object response;

            try
            {
                response = await requestTokensAsync(GetClient());
            }
            catch (ArgumentException e)
            {
                Debugger.Break();
                throw;
            }

            var deserialised = response?.ConvertToMutable<RefreshTokenInfo>();

            if (deserialised is null)
            {
                State = FlowState.Error;
            }
            else
            {
                State = FlowState.TokenGranted;
                RefreshToken = deserialised.RefreshToken;
            }

            return deserialised;
        }

        public async Task<AccessTokenInfo> TryRefreshTokenAsync()
        {
            this.Log("Attempting to refresh access token");
            ThrowIfFaulted();
            State = FlowState.Refreshing;

            object response;

            try
            {
                response = await refreshTokenAsync(GetClient());
            }
            catch (ArgumentException e)
            {
                Debugger.Break();
                throw;
            }

            var deserialised = response?.ConvertToMutable<RefreshTokenInfo>();
            var refreshToken = deserialised?.RefreshToken;

            if (!(refreshToken is null))
            {
                RefreshToken = refreshToken;
            }

            if (deserialised is null)
            {
                // TODO: not working Possibly not connection - check responseMessage please!
                Debugger.Break();
                ErrorState = ErrorState.Connection;
            }
            else
            {
                State = FlowState.TokenRefreshed;
            }

            return deserialised;
        }

        public bool TryTransitionToTokenRequestState(string state, string code, ref string error)
        {
            if (state != StateString)
            {
                ErrorState = ErrorState.StateMismatch;
                error = "state mismatch";
                return false;
            }

            if (error == "access_denied")
            {
                ErrorState = ErrorState.UserDenied;
                return false;
            }

            if (code is null)
            {
                State = FlowState.Error;
                return false;
            }

            State = FlowState.TokenRequest;
            Code = code;
            return true;
        }

        protected abstract object GetClient();

        protected abstract Task<object> refreshTokenAsync(object client);

        protected abstract Task<object> requestTokensAsync(object client);

        private void ThrowIfFaulted()
        {
            if (IsFaulted)
            {
                throw new InvalidOperationException();
            }
        }
    }
}