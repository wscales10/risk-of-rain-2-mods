using Spotify;
using Spotify.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using ZetaIpc.Runtime.Client;
using ZetaIpc.Runtime.Server;

namespace Ror2Mod2
{
	public class SpotifyController<TContext> : MusicBase<TContext>
	{
		private readonly PreferencesLite preferences = new PreferencesLite();

		private readonly IRulePicker<TContext, ICommandList> rulePicker;

		private readonly IContextHelper<TContext> contextHelper;

		public SpotifyController(IRulePicker<TContext, ICommandList> rulePicker, IEnumerable<Playlist> playlists, IContextHelper<TContext> contextHelper, Logger logger) : base(logger)
		{
			this.rulePicker = rulePicker;
			Client = new SpotifyPlaybackClient(playlists, logger, preferences);
			Client.OnError += e => Log(e);
			_ = RunAuthorisationClientAsync();
			this.contextHelper = contextHelper;
			contextHelper.NewContext += c => _ = Update(c);
		}

		private event Action ConfigurationPageRequested;

		protected SpotifyPlaybackClient Client { get; }

		public override void Pause()
		{
			_ = Client.Pause();
		}

		public override void Resume()
		{
			_ = Client.Resume();
		}

		public override void OpenConfigurationPage()
		{
			ConfigurationPageRequested?.Invoke();
		}

		protected override async Task Play(object musicIdentifier)
		{
			if (!(musicIdentifier is null))
			{
				switch (musicIdentifier)
				{
					case Command c:
						await Client.Do(c);
						break;

					case ICommandList commands:
						await Client.Do(commands);
						break;

					default:
						throw new ArgumentException($"Expected a {nameof(ICommandList)} but received a {musicIdentifier.GetType().Name} instead", nameof(musicIdentifier));
				}
			}
		}

		protected override object GetMusicIdentifier(TContext oldContext, TContext newContext)
		{
			var commands = rulePicker.Rule.GetCommands(oldContext, newContext);
			return commands;
		}

		private string[] SendMessage(IpcClient sender, params string[] messages)
		{
			return sender.Send(string.Join("\r\n", messages)).Split(new[] { "\r\n" }, StringSplitOptions.None);
		}

		private async Task RunAuthorisationClientAsync()
		{
			try
			{
				var sender = new IpcClient();
				sender.Initialize(5007);
				Guid guid = Guid.NewGuid();
				var portResponse = SendMessage(sender, $"port | {guid}").Single();

				if (portResponse.Substring(0, 4) != "port")
				{
					throw new NotSupportedException();
				}

				int port = int.Parse(portResponse.Substring(7));
				var receiver = new IpcServer();
				receiver.Start(port);

				foreach (var responsePart in SendMessage(sender, $"conn | {guid}"))
				{
					switch (responsePart.Substring(0, 4))
					{
						case "toke":
							Client.GiftNewAccessToken(responsePart.Substring(7));
							break;

						case "devi":
							preferences.DefaultDeviceString = responsePart.Substring(7);
							break;

						default:
							throw new NotSupportedException();
					}
				}

				receiver.ReceivedRequest += Receiver_ReceivedRequest;
				ConfigurationPageRequested += () => SendMessage(sender, "conf");

				await Task.Delay(Timeout.InfiniteTimeSpan);
			}
			catch (Exception ex)
			{
				Log(ex);
				System.Diagnostics.Debugger.Break();
				throw;
			}

			void Receiver_ReceivedRequest(object _, ReceivedRequestEventArgs e)
			{
				foreach (var request in e.Request.Split(new[] { "\r\n" }, StringSplitOptions.None))
				{
					switch (e.Request.Substring(0, 4))
					{
						case "toke":
							Client.GiftNewAccessToken(e.Request.Substring(7));
							break;

						case "devi":
							preferences.DefaultDeviceString = e.Request.Substring(7);
							break;

						default:
							throw new NotSupportedException();
					}
				}

				e.Handled = true;
			}
		}

		private void OnPipeExceptionOccurred(Exception exception)
		{
			throw exception;
		}
	}
}