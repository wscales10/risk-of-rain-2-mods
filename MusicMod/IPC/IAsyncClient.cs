using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IPC
{
	public interface IAsyncClient : IClient
	{
		Task<string> SendAsync(string message);
	}
}