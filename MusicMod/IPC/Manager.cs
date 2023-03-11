using IPC.Http;

namespace IPC
{
	internal static class Manager
	{
		public static ISender CreateSender(string description)
		{
			return new HttpIpcClient(description);
		}

		public static IReceiver CreateReceiver()
		{
			return new HttpIpcServer();
		}
	}
}