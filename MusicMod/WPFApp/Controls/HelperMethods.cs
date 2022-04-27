using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls
{
	internal static class HelperMethods
	{
		public static void MakeRulesComboBox(ComboBox comboBox, bool selectFirst = false)
		{
			if (selectFirst)
			{
				comboBox.SelectedIndex = 0;
			}

			comboBox.ItemsSource = Info.SupportedRuleTypes;
			comboBox.DisplayMemberPath = nameof(Type.Name);
		}

		public static Color GetColorFromHSL(int H, int S, int L) => System.Drawing.ColorTranslator.FromWin32(ColorHLSToRGB(H, L, S)).ToMediaColor();

		public static BitmapImage ImageFromUri(Uri imageUri)
		{
			BitmapImage bmp = new();
			bmp.BeginInit();
			bmp.UriSource = imageUri;
			bmp.EndInit();
			return bmp;
		}

		public static BitmapImage ImageFromUri(string imageUri) => imageUri is null ? null : ImageFromUri(new Uri(imageUri));

		public static SaveResult All<T>(this IEnumerable<T> source, Func<T, SaveResult> predicate)
		{
			SaveResult result = new(true);

			foreach (SaveResult r in source.Select(predicate))
			{
				if (r is not null)
				{
					result &= r;
				}
			}

			return result;
		}

		[DllImport("shlwapi.dll")]
		private static extern int ColorHLSToRGB(int H, int L, int S);

		private static Color ToMediaColor(this System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
	}
}