using MyRoR2;
using Utils;

namespace SpotifyControlWinForms.Connections
{
	public class RiskOfRain2Connection : IpcConnection
	{
		public RiskOfRain2Connection(IPC.Client client) : base(client)
		{
			MessageReceived += RiskOfRain2Connection_MessageReceived;
		}

		public event Action<RoR2Context>? Output;

		private void RiskOfRain2Connection_MessageReceived(object sender, MessageEventArgs args)
		{
			var message = args.Message;

			switch (message.Key)
			{
				case nameof(RoR2Context):
					Output?.Invoke(Json.FromJson<RoR2Context>(message.Value));
					break;
			}

			args.SetHandled();
		}
	}
}