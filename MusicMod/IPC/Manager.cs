using IPC.Http;

namespace IPC
{
	internal static class Manager
	{
		public static IClient CreateClient()
		{
			return new HttpIpcClient();
		}

		public static IServer CreateServer()
		{
			return new HttpIpcServer();
		}
	}
}