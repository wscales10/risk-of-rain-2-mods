using Spotify;
using System.Windows.Media;

namespace WPFApp.Controls.Wrappers
{
	internal class SpotifyItemWrapper : ControlWrapper<SpotifyItem?, SpotifyItemPicker>
	{
		public override SpotifyItemPicker UIElement { get; } = new SpotifyItemPicker();

		protected override void setValue(SpotifyItem? value) => UIElement.Item = value;

		protected override void SetStatus(bool status) => UIElement.border.BorderBrush = status ? Brushes.Transparent : Brushes.Red;

		protected override bool tryGetValue(out SpotifyItem? value)
		{
			value = UIElement.Item;
			return true;
		}

		protected override bool Validate(SpotifyItem? value) => value is not null;
	}
}