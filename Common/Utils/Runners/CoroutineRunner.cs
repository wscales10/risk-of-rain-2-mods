using System;
using System.Collections;
using Utils.Coroutines;

namespace Utils.Runners
{
	public abstract class CoroutineRunner2
	{
		private readonly RunStateHolder runStateHolder;

		private readonly object lockObject = new object();

		protected CoroutineRunner2(RunState initialState = RunState.Off, bool initialiseWithCorrespondingMethod = false)
		{
			runStateHolder = new RunStateHolder(initialState);
			Info = runStateHolder.ToReadOnly();

			TryStart = new Coroutine2(() => tryStart);

			if (initialiseWithCorrespondingMethod)
			{
				switch (initialState)
				{
					case RunState.Off:
						if (!Coroutine.Run(Stop, (progressUpdate) => true))
						{
							throw new Exception("stop failed");
						}

						break;

					case RunState.Running:
						if (!Coroutine.Run(Start, (progressUpdate) => true))
						{
							throw new Exception("start failed");
						}
						break;

					case RunState.Paused:
						if (!Coroutine.Run(Pause, (progressUpdate) => true))
						{
							throw new Exception("pause failed");
						}
						break;
				}
			}
		}

		public ReadOnlyRunStateHolder Info { get; }

		public Coroutine2 TryStart { get; }

		public bool TryStop(ProgressHandler handler)
		{
			lock (lockObject)
			{
				if (runStateHolder.IsOn && Coroutine.Run(Stop, handler).Success)
				{
					runStateHolder.State = RunState.Off;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool TryResume(ProgressHandler handler)
		{
			lock (lockObject)
			{
				if (runStateHolder.IsPaused && Coroutine.Run(resume, handler).Success)
				{
					runStateHolder.State = RunState.Running;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool TryPause(ProgressHandler handler)
		{
			lock (lockObject)
			{
				if (runStateHolder.IsRunning && Coroutine.Run(resume, handler).Success)
				{
					runStateHolder.State = RunState.Paused;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		protected virtual CoroutineMethod Stop()
		{
			return reference => new[] { true };
		}

		protected virtual CoroutineMethod Start()
		{
			return reference => new[] { true };
		}

		protected virtual CoroutineMethod resume() => Start();

		protected virtual CoroutineMethod Pause()
		{
			return reference => new[] { true };
		}

		private IEnumerable tryStart(Reference reference)
		{
			lock (lockObject)
			{
				var coroutine = new Coroutine2(Start);
				var run = coroutine.Instantiate();

				if (!runStateHolder.IsOn)
				{
					foreach (object progressUpdate in run.Invoke())
					{
						yield return progressUpdate;
					}

					if (run.Result.Success)
					{
						runStateHolder.State = RunState.Running;
						reference.Complete(true);
					}
				}

				reference.Fail();
			}
		}
	}

	public abstract class CoroutineRunner
	{
		private readonly RunStateHolder runStateHolder;

		private readonly object lockObject = new object();

		protected CoroutineRunner(RunState initialState = RunState.Off, bool initialiseWithCorrespondingMethod = false)
		{
			runStateHolder = new RunStateHolder(initialState);
			Info = runStateHolder.ToReadOnly();
			if (initialiseWithCorrespondingMethod)
			{
				switch (initialState)
				{
					case RunState.Off:
						if (!Coroutine.Run(Stop, (progressUpdate) => true))
						{
							throw new Exception("stop failed");
						}

						break;

					case RunState.Running:
						if (!Coroutine.Run(Start, (progressUpdate) => true))
						{
							throw new Exception("start failed");
						}
						break;

					case RunState.Paused:
						if (!Coroutine.Run(Pause, (progressUpdate) => true))
						{
							throw new Exception("pause failed");
						}
						break;
				}
			}
		}

		public ReadOnlyRunStateHolder Info { get; }

		public bool TryStop(ProgressHandler handler)
		{
			lock (lockObject)
			{
				if (runStateHolder.IsOn && Coroutine.Run(Stop, handler).Success)
				{
					runStateHolder.State = RunState.Off;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool TryStart(ProgressHandler handler)
		{
			lock (lockObject)
			{
				if (!runStateHolder.IsOn && Coroutine.Run(Start, handler).Success)
				{
					runStateHolder.State = RunState.Running;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool TryResume(ProgressHandler handler)
		{
			lock (lockObject)
			{
				if (runStateHolder.IsPaused && Coroutine.Run(resume, handler).Success)
				{
					runStateHolder.State = RunState.Running;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool TryPause(ProgressHandler handler)
		{
			lock (lockObject)
			{
				if (runStateHolder.IsRunning && Coroutine.Run(resume, handler).Success)
				{
					runStateHolder.State = RunState.Paused;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		protected virtual CoroutineMethod Stop()
		{
			return reference => new[] { true };
		}

		protected virtual CoroutineMethod Start()
		{
			return reference => new[] { true };
		}

		protected virtual CoroutineMethod resume() => Start();

		protected virtual CoroutineMethod Pause()
		{
			return reference => new[] { true };
		}
	}
}