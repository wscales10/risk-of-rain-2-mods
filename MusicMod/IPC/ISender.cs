namespace IPC
{
	public interface ISender
	{
		string Description { get; }

		void Initialize(int receiverPort);

		string Send(string message);
	}
}