using System.Threading.Tasks;

namespace IPC
{
	public interface IClient
	{
		void Initialize(int serverPort);

		string Send(string message);
	}
}