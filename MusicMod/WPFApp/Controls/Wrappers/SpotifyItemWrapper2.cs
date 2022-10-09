using Spotify;
using WPFApp.Controls.Pickers;

namespace WPFApp.Controls.Wrappers
{
    internal class SpotifyItemWrapper2 : SinglePickerWrapper<ISpotifyItem>
    {
        public SpotifyItemWrapper2(NavigationContext navigationContext) : base(new SpotifyItemPickerInfo(navigationContext))
        {
            UIElement.comboBox.SelectedItem = new TypeWrapper(typeof(SpotifyItem));
        }

        protected override bool Validate(ISpotifyItem value) => base.Validate(value) && value is not null;

        protected override void setValue(ISpotifyItem value)
        {
            IControlWrapper valueWrapper = CreateWrapper(value?.GetType());
            valueWrapper?.SetValue(value);
            UIElement.ViewModel.HandleSelection(valueWrapper);
        }
    }
}