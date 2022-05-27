using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;
using WPFApp.SaveResults;

namespace WPFApp.Controls
{
    internal static class HelperMethods
    {
        public static void MakeRulesComboBox(ComboBox comboBox, bool selectFirst = false) => MakeComboBox(comboBox, Info.SupportedRuleTypes, nameof(Type.Name), selectFirst);

        public static void MakeCommandsComboBox(ComboBox comboBox, bool selectFirst = false) => MakeComboBox(comboBox, Info.SupportedCommandTypes, nameof(Type.Name), selectFirst);

        public static Color GetColorFromHSL(int H, int S, int L) => System.Drawing.ColorTranslator.FromWin32(ColorHLSToRGB(H, L, S)).ToMediaColor();

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

        private static void MakeComboBox(ComboBox comboBox, IEnumerable itemsSource, string displayMemberPath, bool selectFirst)
        {
            if (selectFirst)
            {
                comboBox.SelectedIndex = 0;
            }

            comboBox.ItemsSource = itemsSource;
            comboBox.DisplayMemberPath = displayMemberPath;
        }

        [DllImport("shlwapi.dll")]
        private static extern int ColorHLSToRGB(int H, int L, int S);

        private static Color ToMediaColor(this System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}