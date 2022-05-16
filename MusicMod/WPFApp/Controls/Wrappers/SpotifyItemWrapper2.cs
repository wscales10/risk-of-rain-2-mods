﻿using Spotify;
using WPFApp.Controls.Pickers;

namespace WPFApp.Controls.Wrappers
{
    internal class SpotifyItemWrapper2 : SinglePickerWrapper<ISpotifyItem>
    {
        public SpotifyItemWrapper2(NavigationContext navigationContext) : base(new SpotifyItemPickerInfo(navigationContext))
        {
        }

        protected override void setValue(ISpotifyItem value)
        {
            IControlWrapper valueWrapper = value switch
            {
                SpotifyItem => new SpotifyItemWrapper(),
                PlaylistRef => new PlaylistWrapper(),
                _ => null,
            };

            valueWrapper?.SetValue(value);
            UIElement.ViewModel.HandleSelection(valueWrapper);
        }
    }
}