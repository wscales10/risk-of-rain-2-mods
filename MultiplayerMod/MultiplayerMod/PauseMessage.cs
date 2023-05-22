using RoR2;
using UnityEngine.Networking;

namespace MultiplayerMod
{
	public class PauseMessage : PauseMessageBase
	{
		public PauseMessage() : this(true)
		{
		}

		private PauseMessage(bool toServer) : base(toServer)
		{
		}

		public override void OnReceived()
		{
			if (ToServer)
			{
				Console.instance.SubmitCmd(null, "pause", false);
			}
			else if (!PauseManager.isPaused && NetworkManager.singleton.isNetworkActive && !NetworkServer.active)
			{
				Console.instance.SubmitCmd(null, "pause bypass", false);
			}
		}

		protected override PauseMessageBase Instantiate(bool toServer) => new PauseMessage(toServer);
	}
}