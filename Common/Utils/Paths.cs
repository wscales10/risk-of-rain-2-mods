using System.IO;
using System.Reflection;

namespace Utils
{
	public static class Paths
	{
		public static string AssemblyDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
