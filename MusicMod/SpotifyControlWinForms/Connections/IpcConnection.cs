using IPC;
using Utils;

namespace SpotifyControlWinForms.Connections
{
	public abstract class IpcConnection : ConnectionBase
	{
		private readonly Client client;

		protected IpcConnection(Client client)
		{
			this.client = client;
			client.ReceivedRequest += Client_ReceivedRequest;
			MessageReceived += IpcConnection_MessageReceived;
		}

		protected delegate void MessageHandler(object sender, MessageEventArgs args);

		protected event MessageHandler MessageReceived;

		public override bool Ping()
		{
			return client.PingServer();
		}

		public void SendMessage(string key, string? value = null)
		{
			client.SendToServer(new IPC.Message(key, value));
		}

		protected override bool TryConnect_Inner()
		{
			return client.TryStart.CreateRun().Run(update =>
			{
				this.Log(update.Args);
				return update.Args is not Exception;
			}).Result;
		}

		private void IpcConnection_MessageReceived(object sender, MessageEventArgs args)
		{
			this.Log($"[{DateTime.Now}] {nameof(IpcConnection_MessageReceived)}");
			var message = args.Message;

			switch (message.Key)
			{
				case "pause":
					MusicConnection.Instance.Music.Pause();
					break;

				case "resume":
					MusicConnection.Instance.Music.Resume();
					break;

				default:
					return;
			}

			args.SetHandled();
		}

		private IEnumerable<IPC.Message> Client_ReceivedRequest(IEnumerable<IPC.Message> arg)
		{
			this.Log($"[{DateTime.Now}] {nameof(Client_ReceivedRequest)}");
			List<IPC.Message> output = new();

			foreach (var message in arg)
			{
				List<Func<IPC.Message>> responseGetters = new();
				MessageEventArgs args = new(message, responseGetters);
				MessageReceived?.Invoke(this, args);

				if (!args.Handled)
				{
					throw new NotSupportedException($"message key {message.Key} not supported");
				}

				foreach (var responseGetter in responseGetters)
				{
					output.Add(responseGetter());
				}
			}

			return output;
		}

		protected class MessageEventArgs : EventArgs
		{
			private readonly List<Func<IPC.Message>> responseGetters;

			public MessageEventArgs(IPC.Message message, List<Func<IPC.Message>> responseGetters)
			{
				Message = message;
				this.responseGetters = responseGetters;
			}

			public IPC.Message Message { get; }

			public bool Handled { get; private set; }

			public void AddResponse(Func<IPC.Message> message)
			{
				responseGetters.Add(message);
				SetHandled();
			}

			public void SetHandled()
			{
				Handled = true;
			}
		}
	}
}