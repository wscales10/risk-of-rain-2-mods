using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace MultiplayerMod
{
	public abstract class PauseMessageBase : INetMessage
	{
		protected PauseMessageBase(bool withGuid, bool toServer)
		{
			if (withGuid)
			{
				Guid = System.Guid.NewGuid().ToString();
			}

			ToServer = toServer;
		}

		protected PauseMessageBase(string guid, bool toServer) : this(false, toServer) => Guid = guid;

		public bool ToServer { get; private set; }

		public string Guid { get; private set; }

		public PauseMessageBase ForClient => Instantiate(false);

		public PauseMessageBase ForServer => Instantiate(true);

		public void Deserialize(NetworkReader reader)
		{
			ToServer = reader.ReadBoolean();
			Guid = reader.ReadString();
		}

		public virtual void OnReceived()
		{
			Logging.Record($"Received a {GetType().Name} with GUID {Guid} (ToServer: {ToServer})");
		}

		public void Serialize(NetworkWriter writer)
		{
			writer.Write(ToServer);
			writer.Write(Guid);
		}

		protected abstract PauseMessageBase Instantiate(bool toServer);
	}
}