using System.Collections.Generic;
using System.Reflection;

namespace Utils.Reflection
{
    public static class Resources
    {
        public static byte[] GetByteArray(this Assembly assembly, string resourceName)
        {
            var indexStream = assembly.GetManifestResourceStream(resourceName);

            var arr = new List<byte>();
            int b;

            while ((b = indexStream.ReadByte()) != -1)
            {
                arr.Add((byte)b);
            }

            return arr.ToArray();
        }
    }
}