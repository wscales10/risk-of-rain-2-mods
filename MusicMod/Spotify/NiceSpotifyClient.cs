using Spotify.Commands.Interfaces;
using Spotify.Commands.Mutable;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Async;

namespace Spotify
{
    public abstract class NiceSpotifyClient
    {
        protected readonly Logger Log;

        private readonly AsyncJobQueue asyncJobQueue = new AsyncJobQueue(RunState.Off);

        private string accessToken;

        private string backupAccessToken;

        private bool isAuthorised;

        protected NiceSpotifyClient(Logger logger)
        {
            Log = logger;
            Log("initialising");
            _ = InitialiseAsync().ContinueWith(_ => Log("initialised"), TaskScheduler.Default);
        }

        public event Action<Exception> OnError;

        public bool IsAuthorised
        {
            get => isAuthorised;

            private set
            {
                if (isAuthorised = value)
                {
                    asyncJobQueue.TryStart();
                }
                else
                {
                    asyncJobQueue.TryStop();
                }
            }
        }

        protected SpotifyClient Client { get; private set; }

        public async Task Do(Command input, CancellationToken cancellationToken = default)
        {
            await Do(new CommandList(input).ToReadOnly(), cancellationToken);
        }

        public async Task Do(ICommandList input, CancellationToken cancellationToken = default)
        {
            async Task func(CancellationToken token) => await ExecuteAsync(input, token);
            var cancellableTask = new CancellableTask(func);
            await asyncJobQueue.WaitForMyJobAsync(cancellableTask, cancellationToken);
        }

        public void GiftNewAccessToken(string accessToken)
        {
            backupAccessToken = accessToken;
            if (!IsAuthorised)
            {
                Authorise();
            }
        }

        protected void Throw(Exception e)
        {
            Log($"{e.GetType().FullName}: {e.Message}");
            OnError?.Invoke(e);
        }

        // TODO: make more use of cancellation token
        protected abstract Task<bool> HandleAsync(Command command, CancellationToken cancellationToken);

        protected virtual async Task<bool> HandleErrorAsync(Exception e, ICommandList commands, CancellationToken cancellationToken, List<Type> exceptionTypes = null)
        {
            switch (e)
            {
                case APIUnauthorizedException _:
                case APIException _:
                    if (!exceptionTypes.Contains(e.GetType()))
                    {
                        Authorise();
                        exceptionTypes.Add(e.GetType());
                        return await ExecuteAsync(commands, cancellationToken, exceptionTypes);
                    }

                    IsAuthorised = false;
                    break;
            }

            Throw(e);
            return false;
        }

        protected virtual async Task<bool> ExecuteAsync(ICommandList commands, CancellationToken cancellationToken, List<Type> exceptionTypes = null)
        {
            exceptionTypes = exceptionTypes ?? new List<Type>();
            try
            {
                // TODO: move try-catch inside loop?
                foreach (var command in commands)
                {
                    Log("beginning " + command.GetType().Name);
                    if (!await HandleAsync(command, cancellationToken))
                    {
                        Log(command.GetType().Name + " error");
                        return false;
                    }

                    Log(command.GetType().Name + " completed");
                }

                return true;
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e, commands, cancellationToken, exceptionTypes);
            }
        }

        protected virtual Task InitialiseAsync()
        {
            Authorise();
            return Task.CompletedTask;
        }

        private void Authorise()
        {
            if (backupAccessToken is null)
            {
                IsAuthorised = !((accessToken = Preferences.AccessToken) is null);
            }
            else
            {
                accessToken = backupAccessToken;
                backupAccessToken = null;
                IsAuthorised = true;
            }

            Client = new SpotifyClient(accessToken);
        }
    }
}