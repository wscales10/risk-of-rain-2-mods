using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Utils.Runners
{
	public interface IRunStateHolder
	{
		RunState State { get; }

		bool IsOn { get; }

		bool IsRunning { get; }

		bool IsPaused { get; }

		Task WaitUntilNotRunningAsync();

		Task WaitUntilOffAsync();

		Task WaitUntilRunningAsync();

		Task WaitUntilOnAsync();

		void ThrowIfOff([CallerMemberName] string caller = null);

		void ThrowIfNotRunning([CallerMemberName] string caller = null);
	}
}