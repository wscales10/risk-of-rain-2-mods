using System.Threading.Tasks;

namespace IPC
{
	public interface IAsyncSender : ISender
	{
		Task<string> SendAsync(string message);
	}
}