using System;
using System.Runtime.Serialization;

namespace Utils.Coroutines
{
	[Serializable]
	public class FailedCoroutineRunException : Exception
	{
		public FailedCoroutineRunException()
		{
		}

		public FailedCoroutineRunException(string message) : base(message)
		{
		}

		public FailedCoroutineRunException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected FailedCoroutineRunException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}