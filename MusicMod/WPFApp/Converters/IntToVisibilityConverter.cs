using System;
using System.Windows;
using System.Windows.Data;

namespace WPFApp.Converters
{
    [ValueConversion(typeof(int), typeof(Visibility))]
    public class IntToVisibilityConverter : SimpleValueConverter<int, Visibility>
    {
        private readonly Predicate<int> predicate;

        public IntToVisibilityConverter(Predicate<int> predicate) => this.predicate = predicate;

        protected override Visibility Convert(int value) => predicate(value) ? Visibility.Visible : Visibility.Collapsed;
    }
}