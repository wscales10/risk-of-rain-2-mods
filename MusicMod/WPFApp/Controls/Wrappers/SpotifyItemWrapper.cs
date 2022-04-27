using Spotify;
using System.Windows.Media;

namespace WPFApp.Controls.Wrappers
{
	internal class SpotifyItemWrapper : ControlWrapper<SpotifyItem?, SpotifyItemPicker>
	{
		public SpotifyItemWrapper() => UIElement.ValueChanged += NotifyValueChanged;

		public override SpotifyItemPicker UIElement { get; } = new SpotifyItemPicker();

		protected override void setValue(SpotifyItem? value) => UIElement.Item = value;

		protected override void setStatus(bool status) => UIElement.border.BorderBrush = status ? Brushes.Transparent : Brushes.Red;

		protected override SaveResult<SpotifyItem?> tryGetValue(bool trySave) => new(UIElement.Item);

		protected override bool Validate(SpotifyItem? value) => value is not null;
	}
}