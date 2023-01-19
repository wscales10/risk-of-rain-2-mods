using System;
using System.Runtime.Serialization;

namespace IPC
{
	[Serializable]
	public class SendException : Exception
	{
		private const string message = "Message failed to send.";

		public SendException() : base(message)
		{
		}

		public SendException(Exception innerException) : base(message, innerException)
		{
		}

		protected SendException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}