using Microsoft.VisualStudio.Threading;
using System;
using System.Threading.Tasks;

namespace Utils
{
    public class RunStateHolder : IRunStateHolder
    {
        private readonly AsyncManualResetEvent runningEvent = new AsyncManualResetEvent(true);

        private readonly AsyncManualResetEvent onEvent = new AsyncManualResetEvent(true);

        private RunState state;

        private bool isOn;

        private bool isRunning;

        public RunStateHolder(RunState initialState = RunState.Off) => State = initialState;

        public RunState State
        {
            get => state;

            set
            {
                switch (state = value)
                {
                    case RunState.Off:
                        IsRunning = false;
                        IsOn = false;
                        break;

                    case RunState.Running:
                        IsOn = true;
                        IsRunning = true;
                        break;

                    case RunState.Paused:
                        IsOn = true;
                        IsRunning = false;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        public bool IsOn
        {
            get => isOn;

            private set
            {
                if (isOn != value)
                {
                    if (value)
                    {
                        onEvent.Reset();
                    }
                    else
                    {
                        onEvent.Set();
                    }
                }

                isOn = value;
            }
        }

        public bool IsRunning
        {
            get => isRunning;

            private set
            {
                if (isRunning != value)
                {
                    if (value)
                    {
                        runningEvent.Reset();
                    }
                    else
                    {
                        runningEvent.Set();
                    }
                }

                isRunning = value;
            }
        }

        public bool IsPaused => State == RunState.Paused;

        public ReadOnlyRunStateHolder ToReadOnly() => new ReadOnlyRunStateHolder(this);

        public async Task WaitUntilNotRunningAsync() => await runningEvent.WaitAsync();

        public async Task WaitUntilOffAsync() => await onEvent.WaitAsync();
    }
}