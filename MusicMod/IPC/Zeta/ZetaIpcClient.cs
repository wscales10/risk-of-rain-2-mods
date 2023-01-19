using System.Net;
using IpcClient = ZetaIpc.Runtime.Client.IpcClient;

namespace IPC.Zeta
{
	internal class ZetaIpcClient : IClient
	{
		private readonly IpcClient ipcClient = new IpcClient();

		public void Initialize(int serverPort) => ipcClient.Initialize(serverPort);

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