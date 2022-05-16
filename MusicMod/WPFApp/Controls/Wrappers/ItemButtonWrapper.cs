using System.Windows.Controls;
using WPFApp.Controls.Wrappers.SaveResults;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Wrappers
{
    internal class ItemButtonWrapper : ControlWrapper<IItemViewModel, Button>
    {
        private IItemViewModel outputViewModel;

        public ItemButtonWrapper(Button button) => UIElement = button;

        public override Button UIElement { get; }

        public override bool NeedsRightMargin => false;

        protected override void setValue(IItemViewModel value)
        {
            outputViewModel = value;
        }

        protected override SaveResult<IItemViewModel> tryGetValue(bool trySave)
        {
            return new(outputViewModel);
        }
    }
}