using Spotify;
using Spotify.Commands;
using System;
using WPFApp.Controls.Pickers;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    internal class OffsetWrapper : SinglePickerWrapper<IOffset>
    {
        public OffsetWrapper(NavigationContext navigationContext) : base(new OffsetPickerInfo(navigationContext))
        {
        }

        protected override SaveResult<IOffset> tryGetValue(GetValueRequest request)
        {
            var valueWrapper = UIElement.ViewModel.ValueWrapper;

            if (valueWrapper is null)
            {
                return new(null);
            }

            var result = valueWrapper.TryGetObject(request);

            IOffset offset = valueWrapper switch
            {
                IntWrapper => new IndexOffset { Position = (int)result.Value },
                SpotifyItemWrapper => new ItemOffset { Item = (SpotifyItem)result.Value },
                _ => throw new NotSupportedException(),
            };

            return new SaveResult<IOffset>(result, offset);
        }

        protected override bool Validate(IOffset value) => base.Validate(value) && value is not null;

        protected override void setValue(IOffset value)
        {
            IControlWrapper valueWrapper = CreateWrapper(value?.GetType());

            switch (value)
            {
                case IndexOffset indexOffset:
                    valueWrapper.SetValue(indexOffset.Position);
                    break;

                case ItemOffset itemOffset:
                    valueWrapper.SetValue(itemOffset.Item);
                    break;
            }

            UIElement.ViewModel.HandleSelection(valueWrapper);
        }
    }
}