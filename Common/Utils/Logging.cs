using System.Diagnostics;

namespace Utils
{
	public static class Logging
	{
		public enum LogLevel
		{
			Noise,
			Normal,
			Important
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used to determine type parameter")]
		public static void Log<T>(this T source, object obj, LogLevel logLevel = LogLevel.Normal)
		{
			Debugger.Log((int)logLevel, typeof(T).Name, (obj?.ToString() ?? "null") + "\r\n");
		}
	}
}
