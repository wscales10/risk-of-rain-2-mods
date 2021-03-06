using System.Threading.Tasks;

namespace Utils
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
    }
}