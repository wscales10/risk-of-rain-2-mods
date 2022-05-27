using Patterns.Patterns;
using System.Windows;
using System.Windows.Controls;
using Utils;

namespace WPFApp.Wrappers.PatternWrappers
{
    internal class ListPatternWrapper : ReadOnlyPatternWrapper<IListPattern, Button>
    {
        public ListPatternWrapper(IListPattern pattern, NavigationContext navigationContext) : base(pattern)
        {
            UIElement.Content = pattern.GetType().GetDisplayName(false);
            UIElement.Click += (s, e) => navigationContext.GoInto(pattern);
        }

        public override Button UIElement { get; } = new() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    }
}