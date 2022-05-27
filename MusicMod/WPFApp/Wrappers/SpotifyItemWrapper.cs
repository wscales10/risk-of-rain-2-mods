using Spotify;
using System.Windows.Media;
using WPFApp.Controls;
using WPFApp.Wrappers.SaveResults;

namespace WPFApp.Wrappers
{
    internal class SpotifyItemWrapper : ControlWrapper<SpotifyItem, SpotifyItemPicker>
    {
        public SpotifyItemWrapper()
        {
            UIElement.ViewModel.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(SpotifyItemPickerViewModel.Item):
                        NotifyValueChanged();
                        break;

                    case nameof(SpotifyItemPickerViewModel.Name):
                        NotifyPropertyChanged(nameof(ValueString));
                        break;
                }
            };
        }

        public override bool NeedsRightMargin => false;

        public override SpotifyItemPicker UIElement { get; } = new SpotifyItemPicker();

        public override string ValueString => UIElement.ViewModel.Name;

        protected override void setValue(SpotifyItem value) => UIElement.ViewModel.TrySetItem(value);

        protected override void setStatus(bool status) => UIElement.Border.BorderBrush = status ? Brushes.Transparent : Brushes.Red;

        protected override SaveResult<SpotifyItem> tryGetValue(GetValueRequest request) => new(UIElement.ViewModel.Item);

        protected override bool Validate(SpotifyItem value) => value is not null;
    }
}