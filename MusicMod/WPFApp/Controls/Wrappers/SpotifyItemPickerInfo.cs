using Spotify;
using System;
using System.Collections;
using System.Collections.Generic;
using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.Wrappers
{
    internal class SpotifyItemPickerInfo : IPickerInfo
    {
        private static readonly List<TypeWrapper> allowedItemTypes = new() { typeof(SpotifyItem?), typeof(Playlist) };

        public SpotifyItemPickerInfo(NavigationContext navigationContext)
        {
            NavigationContext = navigationContext;
        }

        public string DisplayMemberPath => nameof(TypeWrapper.DisplayName);

        public string SelectedValuePath => nameof(TypeWrapper.Type);

        public NavigationContext NavigationContext { get; }

        public IEnumerable GetItems()
        {
            return allowedItemTypes;
        }

        public IReadableControlWrapper CreateWrapper(object selectedInfo)
        {
            var type = (Type)selectedInfo;

            if (type == typeof(SpotifyItem?))
            {
                return new SpotifyItemWrapper();
            }

            if (type == typeof(Playlist))
            {
                return new PlaylistWrapper();
            }

            throw new NotSupportedException();
        }
    }
}