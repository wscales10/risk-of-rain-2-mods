using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WPFApp
{
    public static class HttpHelper
    {
        private static readonly HttpClient httpClient = new();

        public static async Task DownloadFileAsync(Uri uri, string outputPath)
        {
            FileInfo fileInfo = new(outputPath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            await File.WriteAllBytesAsync(outputPath, await httpClient.GetByteArrayAsync(uri));
        }
    }
}