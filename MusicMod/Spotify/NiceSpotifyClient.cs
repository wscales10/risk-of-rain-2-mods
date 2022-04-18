using Spotify.Commands;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Utils;

namespace Spotify
{
	public abstract class NiceSpotifyClient
	{
		protected readonly Logger Log;

		private string accessToken;

		private string backupAccessToken;

		private bool isAuthorised;

		private Task<bool> lastQueued;

		protected NiceSpotifyClient(Logger logger)
		{
			Log = logger;
			Log("initialising");
			_ = InitialiseAsync().ContinueWith(_ => Log("initialised"), TaskScheduler.Default);
		}

		public event Action<Exception> OnError;

		protected void Throw(Exception e)
		{
			Log($"{e.GetType().FullName}: {e.Message}");
			OnError?.Invoke(e);
		}

		protected SpotifyClient Client { get; private set; }

		public async Task Do(Command input)
		{
			await Do(new CommandList(input).ToReadOnly());
		}

		public async Task Do(ICommandList input)
		{
			await Execute(t => new CommandListAsync(input, t));
		}

		public void GiftNewAccessToken(string accessToken)
		{
			backupAccessToken = accessToken;
			if (!isAuthorised)
			{
				Authorise();
			}
		}

		protected abstract Task<bool> Handle(Command command);

		protected virtual async Task<bool> ExecuteInner(CommandListAsync wrapper, List<Type> exceptionTypes = null)
		{
			if (!(wrapper.Blocker is null))
			{
				await wrapper.Blocker;
			}

			var commands = wrapper.Commands;
			exceptionTypes = exceptionTypes ?? new List<Type>();
			try
			{
				foreach (var command in commands)
				{
					Log("beginning " + command.GetType().Name);
					if (!await Handle(command))
					{
						Log(command.GetType().Name + " error");
						return false;
					}

					Log(command.GetType().Name + " completed");
				}

				return true;
			}
			catch (APIUnauthorizedException e)
			{
				if (!exceptionTypes.Contains(e.GetType()))
				{
					Authorise();
					exceptionTypes.Add(e.GetType());
					return await ExecuteInner(wrapper, exceptionTypes);
				}

				isAuthorised = false;
				Throw(e);
			}
			catch (Exception e)
			{
				Throw(e);
			}

			return false;
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
				string configFileName = Path.Combine(Paths.AssemblyDirectory, "spotifyAccessToken.txt");
				if (File.Exists(configFileName) && DateTime.UtcNow - File.GetLastWriteTimeUtc(configFileName) < TimeSpan.FromHours(1))
				{
					accessToken = File.ReadAllText(configFileName).Trim();
					isAuthorised = true;
				}
				else
				{
					accessToken = null;
					isAuthorised = false;
				}
			}
			else
			{
				accessToken = backupAccessToken;
				backupAccessToken = null;
				isAuthorised = true;
			}

			Client = new SpotifyClient(accessToken);
		}

		private async Task<bool> Execute(Func<Task, CommandListAsync> f)
		{
			return await (lastQueued = ExecuteInner(f(lastQueued)));
		}
	}
}