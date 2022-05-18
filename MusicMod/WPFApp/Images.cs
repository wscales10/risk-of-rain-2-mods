using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Utils;

namespace WPFApp
{
    public static class Images
    {
        public static event Action CacheUpdated;

        public static string CacheLocation { get; } = Path.Combine(Paths.AssemblyDirectory, "Images");

        public static BitmapImage BuildFromUri(string imageUri)
        {
            if (imageUri is null)
            {
                return null;
            }
            else
            {
                return BuildFromUri(new Uri(imageUri));
            }
        }

        public static BitmapImage BuildFromUri(Uri imageUri)
        {
            BitmapImage bmp = new();
            bmp.BeginInit();
            bmp.UriSource = imageUri;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            return bmp;
        }

        internal static void ClearCache()
        {
            if (!Directory.Exists(CacheLocation))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(CacheLocation))
            {
                File.Delete(file);
            }

            foreach (string directory in Directory.GetDirectories(CacheLocation))
            {
                Directory.Delete(directory);
            }

            CacheUpdated?.Invoke();
        }
    }
}