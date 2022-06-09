using System.Threading.Tasks;

namespace Utils
{
    public class ReadOnlyRunStateHolder : IRunStateHolder
    {
        private readonly RunStateHolder mutable;

        public ReadOnlyRunStateHolder(RunStateHolder mutable) => this.mutable = mutable;

        public RunState State => mutable.State;

        public bool IsOn => mutable.IsOn;

        public bool IsRunning => mutable.IsRunning;

        public bool IsPaused => mutable.IsPaused;

        public Task WaitUntilNotRunningAsync() => mutable.WaitUntilNotRunningAsync();

        public Task WaitUntilOffAsync() => mutable.WaitUntilOffAsync();

        public Task WaitUntilOnAsync() => mutable.WaitUntilOnAsync();

        public Task WaitUntilRunningAsync() => mutable.WaitUntilRunningAsync();
    }
}