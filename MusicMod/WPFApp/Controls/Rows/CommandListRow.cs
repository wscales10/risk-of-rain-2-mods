using Spotify;
using Spotify.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utils.Reflection;
using WPFApp.Controls.CommandControls;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.SaveResults;
using WPFApp.Converters;
using WPFApp.Properties;

namespace WPFApp.Controls.Rows
{
    internal class CommandListRow : Row<Command, CommandListRow>
    {
        private readonly Button previewButton = new()
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14,
            Margin = new Thickness(4),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        private FormatString formatString;

        internal CommandListRow() : base(true)
        {
            previewButton.Click += (s, e) =>
            {
                if (trySaveChanges())
                {
                    _ = OnCommandPreviewRequested?.Invoke(Output);
                }
            };

            OnSetOutput += UpdateButtonLabel;

            Binding binding = new(nameof(Settings.OfflineMode))
            {
                Source = Settings.Default,
                Converter = new InverseBooleanConverter()
            };

            _ = previewButton.SetBinding(UIElement.IsEnabledProperty, binding);

            SetPropertyDependency(nameof(AsString), nameof(FormatString));
        }

        public static event Func<Command, Task> OnCommandPreviewRequested;

        public FormatString FormatString
        {
            get => formatString;

            private set
            {
                if (formatString is not null)
                {
                    RemovePropertyDependency(nameof(AsString), formatString, nameof(formatString.AsString));
                }

                if (value is not null)
                {
                    SetPropertyDependency(nameof(AsString), value, nameof(value.AsString));
                }

                SetProperty(ref formatString, value);
            }
        }

        public string AsString => $"{CommandString}({FormatString?.AsString ?? "..."})";

        public override UIElement LeftElement => previewButton;

        public string CommandString => Output?.GetType().Name.Replace(nameof(Command), string.Empty);

        protected override Command CloneOutput() => Command.FromXml(Output.ToXml());

        protected override CommandListRow deepClone() => new();

        protected override SaveResult trySaveChanges() => (FormatString is null || Output is null) ? (new(false)) : FormatString.TryGetProperties(Output, true);

        protected override UIElement MakeOutputUi()
        {
            if (Output is null)
            {
                ComboBox comboBox = new()
                {
                    FontSize = 14,
                    Margin = new Thickness(40, 4, 4, 4),
                    VerticalAlignment = VerticalAlignment.Center,
                    MinWidth = 150,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                HelperMethods.MakeCommandsComboBox(comboBox);
                comboBox.SelectionChanged += (s, e) =>
                {
                    Output = (Command)((Type)comboBox.SelectedItem).Construct();
                };
                return comboBox;
            }

            FormatString = GetFormatString(Output);
            var stackPanel = FormatString.BuildControl();
            FormatString.SetProperties(Output);
            return stackPanel;
        }

        private static FormatString GetFormatString(Command command)
        {
            switch (command)
            {
                case PlayCommand:
                case LoopCommand:
                case PlayOnceCommand:
                    return FormatString.Create(
                        PropertyString.Create<PlayCommandBase>(true, nameof(PlayCommand.Item)),
                        PropertyString.Create<PlayCommandBase>(false, "at track ", nameof(PlayCommand.Offset)),
                        PropertyString.Create<PlayCommandBase>(false, "at ", nameof(PlayCommand.At)))
                        .Where(c => c.Item is not SpotifyItem item || c.Offset is null || item.Type == SpotifyItemType.Playlist || item.Type == SpotifyItemType.Album);

                case SeekToCommand:
                    return FormatString.Create(PropertyString.Create<SeekToCommand>(true, nameof(SeekToCommand.At)));

                case SetPlaybackOptionsCommand:
                    return FormatString.Create(
                        PropertyString.Create<SetPlaybackOptionsCommand>(false, "Repeat Mode: ", nameof(SetPlaybackOptionsCommand.RepeatMode)),
                        PropertyString.Create<SetPlaybackOptionsCommand>(false, "Shuffle: ", nameof(SetPlaybackOptionsCommand.Shuffle)));

                case SetVolumeCommand:
                    return FormatString.Create(
                        PropertyString.Create<SetVolumeCommand>(true, new IntWrapper(0, 100), "Volume %: ", nameof(SetVolumeCommand.VolumePercent)),
                        PropertyString.Create<SetVolumeCommand>(false, new TextWrapper(new() { Width = 100 }).WithValidation(s => s.Trim().Length > 0), "Volume Control Name: ", nameof(SetVolumeCommand.VolumeControlName)));

                case TransferCommand:
                    return FormatString.Create(
                        PropertyString.Create<TransferCommand>(true, "From ", nameof(TransferCommand.FromTrackId)),
                        PropertyString.Create<TransferCommand>(true, "to ", nameof(TransferCommand.Item)),
                        PropertyString.Create<TransferCommand>(true, new SwitchWrapper<int, string>(TryParse), "at ", nameof(TransferCommand.Mapping))
                        );
                    static bool TryParse(string s, out string output)
                    {
                        output = s;
                        return true;

                        //try
                        //{
                        //    output = SymbolicExpression.Parse(s);
                        //}
                        //catch (Exception)
                        //{
                        //    output = null;
                        //    return false;
                        //}

                        //try
                        //{
                        //    _ = TransferCommand.Map(0, output);
                        //}
                        //catch (Exception e) when (e.Message.StartsWith("Failed to find symbol"))
                        //{
                        //    output = null;
                        //    return false;
                        //}

                        //return true;
                    }
                default:
                    return new FormatString(command?.GetType());
            }
        }

        private void UpdateButtonLabel(Command _) => previewButton.Content = CommandString;
    }
}