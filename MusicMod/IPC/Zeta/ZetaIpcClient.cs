using System.Net;
using IpcClient = ZetaIpc.Runtime.Client.IpcClient;

namespace IPC.Zeta
{
	internal class ZetaIpcClient : ISender
	{
		private readonly IpcClient ipcClient = new IpcClient();

		public ZetaIpcClient(string description)
		{
			Description = description;
		}

		public string Description { get; }

		public int? ServerPort { get; private set; }

		public void Initialize(int serverPort)
		{
			ipcClient.Initialize(serverPort);
			ServerPort = serverPort;
		}

		public string Send(string message)
		{
			try
			{
				return ipcClient.Send(message);
			}
			catch (WebException ex)
			{
				throw new SendException(ex);
			}
		}
	}
}