using System;
using System.Runtime.Serialization;

namespace IPC
{
	[Serializable]
	public class DeliveryException : Exception
	{
		private const string message = "Incorrect address.";

		public DeliveryException(int expectedPort, int actualPort) : base(message)
		{
			ExpectedPort = expectedPort;
			ActualPort = actualPort;
		}

		protected DeliveryException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public int ExpectedPort { get; }

		public int ActualPort { get; }
	}
}