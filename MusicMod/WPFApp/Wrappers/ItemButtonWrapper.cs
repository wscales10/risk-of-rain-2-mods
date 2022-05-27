using System.Windows.Controls;
using WPFApp.Wrappers.SaveResults;

namespace WPFApp.Wrappers
{
    internal class ItemButtonWrapper<TItem> : ControlWrapper<TItem, Button>
    {
        private TItem output;

        public ItemButtonWrapper(Button button) => UIElement = button;

        public override Button UIElement { get; }

        public override bool NeedsRightMargin => false;

        protected override void setValue(TItem value)
        {
            output = value;
        }

        protected override SaveResult<TItem> tryGetValue(GetValueRequest request)
        {
            return new(output);
        }
    }
}