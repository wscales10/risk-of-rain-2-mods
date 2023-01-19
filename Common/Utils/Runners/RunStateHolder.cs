using Microsoft.VisualStudio.Threading;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Utils.Runners
{
	public class RunStateHolder : IRunStateHolder
	{
		private readonly AsyncManualResetEvent notRunningEvent = new AsyncManualResetEvent(true);

		private readonly AsyncManualResetEvent runningEvent = new AsyncManualResetEvent(false);

		private readonly AsyncManualResetEvent offEvent = new AsyncManualResetEvent(true);

		private readonly AsyncManualResetEvent onEvent = new AsyncManualResetEvent(false);

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
						// I'm on now!
						onEvent.Set();

						// Allow "off" notification
						offEvent.Reset();
					}
					else
					{
						// I'm off now!
						offEvent.Set();

						// Allow "on" notification
						onEvent.Reset();
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
						// I'm running now!
						runningEvent.Set();

						// Allow "not running" notifications
						notRunningEvent.Reset();
					}
					else
					{
						// I'm not running now!
						notRunningEvent.Set();

						// Allow "running" notifications
						runningEvent.Reset();
					}
				}

				isRunning = value;
			}
		}

		public bool IsPaused => State == RunState.Paused;

		public ReadOnlyRunStateHolder ToReadOnly() => new ReadOnlyRunStateHolder(this);

		public async Task WaitUntilNotRunningAsync() => await notRunningEvent.WaitAsync();

		public async Task WaitUntilOffAsync() => await offEvent.WaitAsync();

		public async Task WaitUntilRunningAsync() => await runningEvent.WaitAsync();

		public async Task WaitUntilOnAsync() => await onEvent.WaitAsync();

		public void ThrowIfOff([CallerMemberName] string caller = null)
		{
			if (!IsOn)
			{
				throw new InvalidOperationException($"This {GetType().GetDisplayName()} does not allow {caller} while off.");
			}
		}

		public void ThrowIfNotRunning([CallerMemberName] string caller = null)
		{
			if (!IsRunning)
			{
				throw new InvalidOperationException($"This {GetType().GetDisplayName()} does not allow {caller} while not running.");
			}
		}
	}
}