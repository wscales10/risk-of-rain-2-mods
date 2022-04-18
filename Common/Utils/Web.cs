using System;
using System.Diagnostics;

namespace Utils
{
	public static class Web
	{
		public static void Goto(Uri uri)
		{
			Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
		}
	}
}
