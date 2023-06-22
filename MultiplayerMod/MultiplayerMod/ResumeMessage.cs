using RoR2;
using UnityEngine.Networking;

namespace MultiplayerMod
{
	public class ResumeMessage : PauseMessageBase
	{
		public ResumeMessage() : this(false)
		{
		}

		private ResumeMessage(string guid, bool toServer) : base(guid, toServer)
		{
		}

		private ResumeMessage(bool withGuid, bool toServer = true) : base(withGuid, toServer)
		{
		}

		public static ResumeMessage Create()
		{
			ResumeMessage output = new ResumeMessage(true);
			Logging.Record($"Created a new {nameof(ResumeMessage)} with GUID {output.Guid}");
			return output;
		}

		public override void OnReceived()
		{
			base.OnReceived();

			if (ToServer)
			{
				Console.instance.SubmitCmd(null, "pause resume", false);
				return;
			}

			if (!PauseManager.isPaused)
			{
				Logging.Record("PauseManager is not paused, so cannot resume");
				return;
			}

			if (NetworkServer.active)
			{
				Logging.Record("Message for clients receieved by server, not processing");
				return;
			}

			Console.instance.SubmitCmd(null, "pause resume-bypass", false);
		}

		protected override PauseMessageBase Instantiate(bool toServer) => new ResumeMessage(Guid, toServer);
	}
}