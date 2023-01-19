using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Utils.Runners
{
	public class ReadOnlyRunStateHolder : IRunStateHolder
	{
		private readonly RunStateHolder mutable;

		public ReadOnlyRunStateHolder(RunStateHolder mutable) => this.mutable = mutable;

		public RunState State => mutable.State;

		public bool IsOn => mutable.IsOn;

		public bool IsRunning => mutable.IsRunning;

		public bool IsPaused => mutable.IsPaused;

		public void ThrowIfNotRunning([CallerMemberName] string caller = null) => mutable.ThrowIfNotRunning(caller);

		public void ThrowIfOff([CallerMemberName] string caller = null) => mutable.ThrowIfOff(caller);

		public Task WaitUntilNotRunningAsync() => mutable.WaitUntilNotRunningAsync();

		public Task WaitUntilOffAsync() => mutable.WaitUntilOffAsync();

		public Task WaitUntilOnAsync() => mutable.WaitUntilOnAsync();

		public Task WaitUntilRunningAsync() => mutable.WaitUntilRunningAsync();
	}
}