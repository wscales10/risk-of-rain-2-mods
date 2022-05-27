using Spotify;
using System;
using System.Collections.Generic;

namespace WPFApp.ViewModels.Pickers
{
    internal class SpotifyItemPickerInfo : PickerInfoBase<TypeWrapper>
    {
        private static readonly List<TypeWrapper> allowedItemTypes = new() { typeof(SpotifyItem), typeof(PlaylistRef) };

        public SpotifyItemPickerInfo(NavigationContext navigationContext) : base(navigationContext)
        {
        }

        public override string DisplayMemberPath => nameof(TypeWrapper.DisplayName);

        public override string SelectedValuePath => nameof(TypeWrapper.Type);

        public override IEnumerable<TypeWrapper> GetItems() => allowedItemTypes;

        public override object CreateItem(TypeWrapper selectedType)
        {
            var type = selectedType.Type;

            if (type == typeof(SpotifyItem))
            {
                return new SpotifyItemWrapper();
            }

            if (type == typeof(PlaylistRef))
            {
                return new PlaylistWrapper();
            }

            throw new NotSupportedException();
        }
    }
}