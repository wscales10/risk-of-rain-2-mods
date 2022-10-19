using Spotify;
using System.Windows;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Rows
{
    internal class SpotifyItemRow : Row<SpotifyItem, SpotifyItemRow>
    {
        private readonly SpotifyItemWrapper spotifyItemWrapper = new();

        private bool isInitialised;

        public SpotifyItemRow() : base(true)
        {
        }

		public override SpotifyItem Output
		{
			get
			{
				var value = spotifyItemWrapper.TryGetValue(false).Value;

				if (base.Output != value)
				{
					base.Output = value;
				}

				return value;
			}

			set
			{
				if (!isInitialised)
				{
					spotifyItemWrapper.ValueSet += SpotifyItemWrapper_ValueSet;
					isInitialised = true;
				}

				spotifyItemWrapper.SetValue(value);
				NotifyPropertyChanged();
			}
		}

		protected override SaveResult<SpotifyItem> trySaveChanges()
        {
            var result = spotifyItemWrapper.TryGetValue(true);

            base.Output = result.Value;

            if (result.Value?.Type == SpotifyItemType.Track)
            {
                return result;
            }
            else
            {
                return new(false);
            }
        }

        protected override UIElement MakeOutputUi()
        {
            var output = spotifyItemWrapper.UIElement;
            output.Margin = new Thickness(40, 4, 4, 4);
            output.HorizontalAlignment = HorizontalAlignment.Left;
            return output;
        }

        protected override SpotifyItem CloneOutput() => new(Output.Type, Output.Id);

        protected override SpotifyItemRow deepClone() => new();

        private void SpotifyItemWrapper_ValueSet(object obj) => base.Output = (SpotifyItem)obj;
    }
}