using Spotify;
using Spotify.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Pickers
{
    internal class OffsetPickerInfo : IPickerInfo
    {
        private static readonly List<TypeWrapper> allowedItemTypes = new() { typeof(IndexOffset), typeof(ItemOffset) };

        public OffsetPickerInfo(NavigationContext navigationContext)
        {
            NavigationContext = navigationContext;
        }

        public string DisplayMemberPath => nameof(TypeWrapper.DisplayName);

        public string SelectedValuePath => nameof(TypeWrapper.Type);

        public NavigationContext NavigationContext { get; }

        public IEnumerable GetItems() => allowedItemTypes;

        public IReadableControlWrapper CreateWrapper(object selectedInfo)
        {
            var type = (Type)selectedInfo;

            if (type == typeof(IndexOffset))
            {
                IntWrapper intWrapper = new();
                intWrapper.OnValidate += (_, e) => e.Extend(new(e.SaveResult.Value >= 0));
                return intWrapper;
            }

            if (type == typeof(ItemOffset))
            {
                SpotifyItemWrapper spotifyItemWrapper = new();
                spotifyItemWrapper.OnValidate += (_, e) => e.Extend(new(e.SaveResult.Value?.Type == SpotifyItemType.Track));
                return spotifyItemWrapper;
            }

            return null;
        }
    }
}