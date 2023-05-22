using RoR2;
using UnityEngine.Networking;

namespace MultiplayerMod
{
	public class ResumeMessage : PauseMessageBase
	{
		public ResumeMessage() : this(true)
		{
		}

		public ResumeMessage(bool toServer) : base(toServer)
		{
		}

		public override void OnReceived()
		{
			if (ToServer)
			{
				Console.instance.SubmitCmd(null, "pause", false);
			}
			else if (PauseManager.isPaused && !NetworkServer.active)
			{
				Console.instance.SubmitCmd(null, "pause bypass", false);
			}
		}

		protected override PauseMessageBase Instantiate(bool toServer) => new ResumeMessage(toServer);
	}
}