namespace IPC
{
	public interface IIpcManager
	{
		IClient CreateClient();

		IServer CreateServer();

		int GetFreePort();
	}
}