using RoR2;
using UnityEngine.Networking;

namespace MultiplayerMod
{
	public class PauseMessage : PauseMessageBase
	{
		public PauseMessage() : this(false)
		{
		}

		private PauseMessage(string guid, bool toServer) : base(guid, toServer)
		{
		}

		private PauseMessage(bool withGuid, bool toServer = true) : base(withGuid, toServer)
		{
		}

		public static PauseMessage Create()
		{
			PauseMessage output = new PauseMessage(true);
			Logging.Record($"Created a new {nameof(PauseMessage)} with GUID {output.Guid}");
			return output;
		}

		public override void OnReceived()
		{
			base.OnReceived();

			if (ToServer)
			{
				Console.instance.SubmitCmd(null, "pause pause", false);
				return;
			}

			if (PauseManager.isPaused)
			{
				Logging.Record("PauseManager is paused, so cannot pause");
				return;
			}

			if (!NetworkManager.singleton.isNetworkActive)
			{
				Logging.Record("NetworkManager is not active, so custom pause logic not required");
				return;
			}

			if (NetworkServer.active)
			{
				Logging.Record("Message for clients receieved by server, not processing");
				return;
			}

			Console.instance.SubmitCmd(null, "pause pause-bypass", false);
		}

		protected override PauseMessageBase Instantiate(bool toServer) => new PauseMessage(Guid, toServer);
	}
}