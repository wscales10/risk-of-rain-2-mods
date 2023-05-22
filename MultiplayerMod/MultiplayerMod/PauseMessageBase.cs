using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace MultiplayerMod
{
	public abstract class PauseMessageBase : INetMessage
	{
		protected PauseMessageBase(bool toServer)
		{
			ToServer = toServer;
		}

		public bool ToServer { get; private set; }

		public PauseMessageBase ForClient => Instantiate(false);

		public PauseMessageBase ForServer => Instantiate(true);

		public void Deserialize(NetworkReader reader)
		{
			ToServer = reader.ReadBoolean();
		}

		public abstract void OnReceived();

		public void Serialize(NetworkWriter writer)
		{
			writer.Write(ToServer);
		}

		protected abstract PauseMessageBase Instantiate(bool toServer);
	}
}