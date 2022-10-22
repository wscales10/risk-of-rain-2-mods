using Spotify.Commands;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Async;
using Utils.Runners;

namespace Spotify
{
	public abstract class NiceSpotifyClient
	{
		protected readonly Logger Log;

		protected readonly IPreferences preferences;

		private readonly AsyncJobQueue asyncJobQueue = new AsyncJobQueue(RunState.Off);

		private string accessToken;

		private bool isAuthorised;

		protected NiceSpotifyClient(Logger logger, IPreferences preferences)
		{
			Log = logger;
			(this.preferences = preferences).PropertyChanged += (name) =>
			{
				switch (name)
				{
					case nameof(IPreferences.AccessToken):
						if (!IsAuthorised)
						{
							TryAuthorise();
						}
						break;
				}
			};
			Log("initialising");
			_ = InitialiseAsync().ContinueWith(_ => Log("initialised"), TaskScheduler.Default);
		}

		public event Action<Exception> OnError;

		public event Action OnAccessTokenRequested;

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

			if (!IsAuthorised)
			{
				OnAccessTokenRequested?.Invoke();
			}

			await asyncJobQueue.WaitForMyJobAsync(cancellableTask, cancellationToken);
		}

		protected void Throw(Exception e)
		{
			Log($"{e.GetType().FullName}: {e.Message}");
			OnError?.Invoke(e);
		}

		// TODO: make more use of cancellation token
		protected abstract Task<bool> HandleAsync(Command command, CancellationToken cancellationToken);

		protected virtual async Task<bool> HandleErrorAsync(Exception e, ICommandList commands, CancellationToken cancellationToken, List<SpotifyError> errors = null)
		{
			var error = ClassifyException(e);
			switch (e)
			{
				case APIUnauthorizedException _:
				case APIException _:

					switch (errors.Count(t => t == error))
					{
						case 0:
							TryAuthorise();
							errors.Add(error);
							return await ExecuteAsync(commands, cancellationToken, errors);

						case 1:
							IsAuthorised = false;
							OnAccessTokenRequested?.Invoke();
							return await ExecuteAsync(commands, cancellationToken, errors);
					}

					IsAuthorised = false;
					break;
			}

			Throw(e);
			return false;
		}

		protected virtual SpotifyError ClassifyException(Exception e)
		{
			return new SpotifyError(e.GetType());
		}

		protected virtual async Task<bool> ExecuteAsync(ICommandList commands, CancellationToken cancellationToken, List<SpotifyError> exceptionTypes = null)
		{
			exceptionTypes = exceptionTypes ?? new List<SpotifyError>();
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
			TryAuthorise();
			return Task.CompletedTask;
		}

		private bool TryAuthorise()
		{
			if (!(preferences.AccessToken is null))
			{
				Client = new SpotifyClient(accessToken = preferences.AccessToken);
				return IsAuthorised = true;
			}

			return false;
		}
	}
}