using Spotify;
using System.Windows.Media;

namespace WPFApp.Controls.Wrappers
{
    internal class SpotifyItemWrapper : ControlWrapper<SpotifyItem?, SpotifyItemPicker>
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

        public override SpotifyItemPicker UIElement { get; } = new SpotifyItemPicker();

        public override string ValueString => UIElement.ViewModel.Name;

        protected override void setValue(SpotifyItem? value) => UIElement.ViewModel.Item = value;

        protected override void setStatus(bool status) => UIElement.Border.BorderBrush = status ? Brushes.Transparent : Brushes.Red;

        protected override SaveResult<SpotifyItem?> tryGetValue(bool trySave) => new(UIElement.ViewModel.Item);

        protected override bool Validate(SpotifyItem? value) => value is not null;
    }
}