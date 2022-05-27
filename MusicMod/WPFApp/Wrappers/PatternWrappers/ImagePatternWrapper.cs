using Patterns.Patterns.SmallPatterns;
using System.Windows.Controls;
using WPFApp.Controls.PatternControls;
using WPFApp.ViewModels;

namespace WPFApp.Wrappers.PatternWrappers
{
    internal abstract class ImagePatternWrapper<TPattern> : ValuePatternWrapper<TPattern, ImagePatternControl>
        where TPattern : IValuePattern
    {
        protected ImagePatternWrapper(TPattern pattern) : base(pattern)
        {
            UIElement.DataContext = ViewModel;
        }

        public sealed override ImagePatternControl UIElement { get; } = new();

        public override bool NeedsRightMargin => false;

        protected abstract ImagePatternViewModel ViewModel { get; }

        protected sealed override TextBox TextBox => UIElement.textBox;

        protected override string Text { get => ViewModel.Text; set => ViewModel.Text = value; }
    }
}