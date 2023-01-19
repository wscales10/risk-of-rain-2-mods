using System.Collections.Generic;

namespace Utils.Coroutines
{
	public delegate bool ProgressHandler(object progressUpdate);

	public delegate IEnumerable<ProgressUpdate> CoroutineMethod(Reference reference);

	public class Coroutine
	{
		private readonly CoroutineMethod method;

		public Coroutine(CoroutineMethod method) => this.method = method;

		public static IEnumerable<ProgressUpdate> DefaultMethod(Reference reference)
		{
			reference.Complete();
			yield break;
		}

		public CoroutineRun CreateRun()
		{
			return new CoroutineRun(method);
		}
	}
}