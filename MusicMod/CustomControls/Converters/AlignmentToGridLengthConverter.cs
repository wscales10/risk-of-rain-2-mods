using System;
using System.Windows;
using System.Windows.Data;

namespace CustomControls.Converters
{
    [ValueConversion(typeof(HorizontalAlignment), typeof(GridLength))]
    public class AlignmentToGridLengthConverter : SimpleValueConverter<HorizontalAlignment, GridLength>
    {
        public GridLength Left { get; set; }

        public GridLength Right { get; set; }

        public GridLength Center { get; set; }

        public GridLength Stretch { get; set; }

        protected override GridLength Convert(HorizontalAlignment value) => value switch
        {
            HorizontalAlignment.Left => Left,
            HorizontalAlignment.Center => Center,
            HorizontalAlignment.Right => Right,
            HorizontalAlignment.Stretch => Stretch,
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }
}