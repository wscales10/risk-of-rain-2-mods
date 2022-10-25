using System.Linq;

namespace Utils.Runners
{
	public abstract class Runner
	{
		private readonly RunStateHolder runStateHolder;

		private readonly object lockObject = new object();

		protected Runner(RunState initialState = RunState.Off, bool initialiseWithCorrespondingMethod = false)
		{
			runStateHolder = new RunStateHolder(initialState);
			Info = runStateHolder.ToReadOnly();
			if (initialiseWithCorrespondingMethod)
			{
				switch (initialState)
				{
					case RunState.Off:
						Stop();
						break;

					case RunState.Running:
						Start();
						break;

					case RunState.Paused:
						Pause();
						break;
				}
			}
		}

		public ReadOnlyRunStateHolder Info { get; }

		public bool TryStop()
		{
			lock (lockObject)
			{
				if (runStateHolder.IsOn)
				{
					Stop();
					runStateHolder.State = RunState.Off;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool TryStart()
		{
			lock (lockObject)
			{
				if (!runStateHolder.IsOn)
				{
					Start();
					runStateHolder.State = RunState.Running;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool TryResume()
		{
			lock (lockObject)
			{
				if (runStateHolder.IsPaused)
				{
					Resume();
					runStateHolder.State = RunState.Running;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool TryPause()
		{
			lock (lockObject)
			{
				if (runStateHolder.IsRunning)
				{
					Pause();
					runStateHolder.State = RunState.Paused;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		protected virtual void Stop()
		{
		}

		protected virtual void Start()
		{
		}

		protected virtual void Resume() => Start();

		protected virtual void Pause()
		{
		}
	}
}