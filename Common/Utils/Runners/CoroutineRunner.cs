using System;
using System.Collections.Generic;
using Utils.Coroutines;

namespace Utils.Runners
{
	public abstract class CoroutineRunner
	{
		private readonly RunStateHolder runStateHolder;

		private readonly object lockObject = new object();

		protected CoroutineRunner(RunState initialState = RunState.Off, bool initialiseWithCorrespondingMethod = false)
		{
			runStateHolder = new RunStateHolder(initialState);
			Info = runStateHolder.ToReadOnly();

			TryStart = new Coroutine(tryStart);
			TryResume = new Coroutine(tryResume);
			TryStop = new Coroutine(tryStop);
			TryPause = new Coroutine(tryPause);

			if (initialiseWithCorrespondingMethod)
			{
				CoroutineRun run;
				switch (initialState)
				{
					case RunState.Off:
						run = new Coroutine(Stop).CreateRun().RunToCompletion();

						if (!run.Result.Success)
						{
							throw new Exception("stop failed");
						}

						break;

					case RunState.Running:
						run = new Coroutine(Start).CreateRun().RunToCompletion();

						if (!run.Result.Success)
						{
							throw new Exception("start failed");
						}

						break;

					case RunState.Paused:
						run = new Coroutine(Pause).CreateRun().RunToCompletion();

						if (!run.Result.Success)
						{
							throw new Exception("pause failed");
						}

						break;
				}
			}
		}

		public ReadOnlyRunStateHolder Info { get; }

		public Coroutine TryStart { get; }

		public Coroutine TryStop { get; }

		public Coroutine TryResume { get; }

		public Coroutine TryPause { get; }

		public IEnumerable<ProgressUpdate> tryStop(Reference reference)
		{
			lock (lockObject)
			{
				var run = new Coroutine(Stop).CreateRun();

				if (runStateHolder.IsOn)
				{
					foreach (var progressUpdate in run.GetProgressUpdates())
					{
						yield return progressUpdate;
					}

					if (run.Result.Success)
					{
						runStateHolder.State = RunState.Off;
						reference.Complete();
						yield break;
					}
				}

				reference.Fail();
			}
		}

		public IEnumerable<ProgressUpdate> tryResume(Reference reference)
		{
			lock (lockObject)
			{
				var run = new Coroutine(Resume).CreateRun();

				if (runStateHolder.IsPaused)
				{
					foreach (var progressUpdate in run.GetProgressUpdates())
					{
						yield return progressUpdate;
					}

					if (run.Result.Success)
					{
						runStateHolder.State = RunState.Running;
						reference.Complete();
						yield break;
					}
				}

				reference.Fail();
			}
		}

		public IEnumerable<ProgressUpdate> tryPause(Reference reference)
		{
			lock (lockObject)
			{
				var run = new Coroutine(Pause).CreateRun();

				if (runStateHolder.IsRunning)
				{
					foreach (var progressUpdate in run.GetProgressUpdates())
					{
						yield return progressUpdate;
					}

					if (run.Result.Success)
					{
						runStateHolder.State = RunState.Paused;
						reference.Complete();
						yield break;
					}
				}

				reference.Fail();
			}
		}

		protected virtual IEnumerable<ProgressUpdate> Resume(Reference reference) => Start(reference);

		protected virtual IEnumerable<ProgressUpdate> Pause(Reference reference) => Coroutine.DefaultMethod(reference);

		protected virtual IEnumerable<ProgressUpdate> Start(Reference reference) => Coroutine.DefaultMethod(reference);

		protected virtual IEnumerable<ProgressUpdate> Stop(Reference reference) => Coroutine.DefaultMethod(reference);

		private IEnumerable<ProgressUpdate> tryStart(Reference reference)
		{
			lock (lockObject)
			{
				var run = new Coroutine(Start).CreateRun();

				if (!runStateHolder.IsOn)
				{
					foreach (ProgressUpdate progressUpdate in run.GetProgressUpdates())
					{
						yield return progressUpdate;
					}

					if (run.Result.Success)
					{
						runStateHolder.State = RunState.Running;
						reference.Complete(true);
						yield break;
					}
				}

				reference.Fail();
			}
		}
	}
}