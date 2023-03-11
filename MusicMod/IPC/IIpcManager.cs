namespace IPC
{
	public interface IIpcManager
	{
		ISender CreateSender();

		IReceiver CreateReceiver();
	}
}