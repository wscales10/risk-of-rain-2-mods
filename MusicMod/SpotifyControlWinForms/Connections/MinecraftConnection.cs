using Minecraft;
using Utils;

namespace SpotifyControlWinForms.Connections
{
	public class MinecraftConnection : IpcConnection
	{
		public MinecraftConnection(IPC.Client client) : base(client)
		{
			MessageReceived += MinecraftConnection_MessageReceived;
		}

		public event Action<MinecraftContext>? Output;

		private void MinecraftConnection_MessageReceived(object sender, MessageEventArgs args)
		{
			this.Log($"[{DateTime.Now}] {nameof(MinecraftConnection_MessageReceived)}");
			var message = args.Message;

			switch (message.Key)
			{
				case nameof(MinecraftContext):
					Output?.Invoke(Json.FromJson<MinecraftContext>(message.Value, settings => settings.Error = (sender, args) =>
					{
						if (System.Diagnostics.Debugger.IsAttached)
						{
							System.Diagnostics.Debugger.Break();
						}
					}));
					args.SetHandled();
					break;
			}
		}
	}
}