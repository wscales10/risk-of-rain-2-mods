using System;

namespace IPC
{
	public interface IServer
	{
		event Func<string, string> ReceivedRequest;

		void Start(int port);
	}
}