using IPC;
using Spotify.Authorisation;

namespace SpotifyAuthenticationWinForms
{
    internal class SpotifyAuthenticationServer
    {
        private static readonly string logFilePath = "log.txt";

        private readonly IPC.Server server = new(5007, nameof(SpotifyAuthenticationServer));

        private string? lastToken;

        private Authorisation? authorisation;

        internal SpotifyAuthenticationServer()
        {
            ClearLogFile();
            server.ReceivedRequest += Server_ReceivedRequest;
            server.OnAddClient += Server_OnAddClient;
            server.TryStart.CreateRun().RunToCompletion();
            OnAccessTokenReceived += BroadcastToken;

            Form = new();
            Form.OnRestart += () => _ = Restart();
            Form.OnConfigure += () => authorisation?.OpenConfigurationPage();
            Form.OnResetAccess += () => { if (authorisation is not null) authorisation.Preferences.AccessToken = null; };
            Form.OnResetRefresh += () => { if (authorisation is not null) authorisation.Preferences.RefreshToken = null; };
            Form.FormClosed += (s, e) => Application.Exit();
            SetupAuthorisation();
        }

        private event Authorisation.AccessTokenHandler? OnAccessTokenReceived;

        public Form1 Form { get; }

        private static void ClearLogFile() => File.Create(logFilePath).Close();

        private IEnumerable<IPC.Message> Server_OnAddClient(string guid)
        {
            if (authorisation is not null)
            {
                authorisation.Preferences.PropertyChanged += (name) =>
                {
                    if (name == nameof(authorisation.Preferences.DefaultDevice))
                    {
                        server.SendToClient(guid, new IPC.Message("devi", authorisation.Preferences.DefaultDeviceString));
                    }
                };
            }

            if (lastToken is not null)
            {
                yield return new("toke", lastToken);
            }

            if (!string.IsNullOrWhiteSpace(authorisation?.Preferences.DefaultDeviceString))
            {
                yield return new("devi", authorisation.Preferences.DefaultDeviceString);
            }
        }

        private void BroadcastToken(Authorisation _, string accessToken)
        {
            lastToken = accessToken;
            server.Broadcast(new IPC.Message("toke", accessToken));
        }

        private async Task Restart()
        {
            try
            {
                if (authorisation is null)
                {
                    throw new InvalidOperationException($"{nameof(authorisation)} is null");
                }

                LogInfo("Stop Authorisation");
                await authorisation.TryStopAsync();
                LogInfo("Start Authorisation");
                await authorisation.TryStartAsync();
                LogInfo("Authorisation Restarted");
            }
            catch
            {
                SetupAuthorisation();
            }
        }

        private void SetupAuthorisation()
        {
            LogInfo("New Authorisation");
            authorisation = new(Scopes.Playback, logger: x => LogInfo(x?.ToString() ?? "[[null]]"));
            authorisation.OnClientRequested += Utils.Web.Goto;
            authorisation.FlowStateChanged += Form.Authorisation_FlowStateChanged;
            authorisation.ErrorStateChanged += Form.Authorisation_ErrorStateChanged;
            authorisation.Preferences.PropertyChanged += (name) =>
            {
                if (name == nameof(authorisation.Preferences.DefaultDevice))
                {
                    server.Broadcast(new IPC.Message("devi", authorisation.Preferences.DefaultDeviceString));
                }
                else if (name == nameof(authorisation.Preferences.AccessToken))
                {
                    OnAccessTokenReceived?.Invoke(authorisation, authorisation.Preferences.AccessToken);
                }
            };

            _ = authorisation.InitiateScopeRequestAsync().ContinueWith(_ =>
            {
                if (authorisation.Startup != Authorisation.AuthorisationStartup.Full)
                {
                    BroadcastToken(authorisation, authorisation.Preferences.AccessToken);
                }
            }, TaskScheduler.Default);

            Form.RestartButtonEnabled = true;
        }

        private void OnExceptionOccurred(Exception exception)
        {
            LogInfo(exception.Message);
            throw exception;
        }

        private IEnumerable<IPC.Message> Server_ReceivedRequest(IEnumerable<IPC.Message> request)
        {
            foreach (var requestPart in request)
            {
                switch (requestPart.Key)
                {
                    case "conf":
                        authorisation?.OpenConfigurationPage();
                        break;

                    case "toke":
                        yield return new("toke", authorisation?.Preferences.AccessToken);
                        break;

                    case "devi":
                        yield return new("devi", authorisation?.Preferences.DefaultDeviceString);
                        break;
                }
            }
        }

        private void LogInfo(string message)
        {
            Utils.Logging.Log(this, message);
            File.AppendAllLines(logFilePath, new[] { message });
        }
    }
}