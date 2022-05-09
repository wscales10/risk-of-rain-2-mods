using Spotify;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.Wrappers
{
    internal abstract class SinglePickerWrapper<T> : ControlWrapper<T, SinglePicker>
    {
        protected SinglePickerWrapper(IPickerInfo config)
        {
            NavigationContext = config.NavigationContext;
            UIElement = new(new(config));
            UIElement.ViewModel.ValueChanged += NotifyValueChanged;
            UIElement.ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            SetPropertyDependency(nameof(ValueString), UIElement.ViewModel, nameof(UIElement.ViewModel.ValueWrapper));
            SetValue(null);
        }

        public sealed override SinglePicker UIElement { get; }

        public override string ValueString => UIElement.ViewModel.ValueWrapper?.ValueString;

        protected NavigationContext NavigationContext { get; }

        protected sealed override SaveResult<T> tryGetValue(bool trySave)
        {
            var valueWrapper = UIElement.ViewModel.ValueWrapper;

            if (valueWrapper is null)
            {
                return new(false);
            }

            return SaveResult.Create<T>(valueWrapper.TryGetObject(trySave));
        }

        protected sealed override void setStatus(bool? status) => Outline(UIElement.comboBox, status is null ? null : UIElement.ViewModel.ValueWrapper is not null);

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SinglePickerViewModel.ValueWrapper):
                    var viewModel = (SinglePickerViewModel)sender;
                    RemovePropertyDependency(nameof(ValueString), nameof(IControlWrapper.ValueString));
                    SetPropertyDependency(nameof(ValueString), viewModel.ValueWrapper, nameof(IControlWrapper.ValueString));
                    break;
            }
        }
    }

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
                Playlist => new PlaylistWrapper(),
                _ => null,
            };

            valueWrapper?.SetValue(value);
            UIElement.ViewModel.HandleSelection(valueWrapper);
        }
    }

    internal class PlaylistWrapper : ControlWrapper<Playlist, ComboBox>
    {
        public PlaylistWrapper()
        {
            UIElement.ItemsSource = Info.Playlists;
            UIElement.DisplayMemberPath = nameof(Playlist.Name);
        }

        public override ComboBox UIElement { get; } = new();

        protected override void setValue(Playlist value) => UIElement.SelectedItem = value;

        protected override SaveResult<Playlist> tryGetValue(bool trySave)
        {
            var playlist = (Playlist)UIElement.SelectedItem;
            return playlist is null ? (new(false)) : (new(playlist));
        }
    }

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

        protected override void setValue(SpotifyItem? value) => UIElement.ViewModel.TrySetItem(value);

        protected override void setStatus(bool status) => UIElement.Border.BorderBrush = status ? Brushes.Transparent : Brushes.Red;

        protected override SaveResult<SpotifyItem?> tryGetValue(bool trySave) => new(UIElement.ViewModel.Item);

        protected override bool Validate(SpotifyItem? value) => value is not null;
    }
}