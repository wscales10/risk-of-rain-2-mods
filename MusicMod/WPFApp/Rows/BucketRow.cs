using Spotify.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utils.Reflection;
using WPFApp.Controls;
using WPFApp.SaveResults;
using WPFApp.Properties;
using System.Windows.Input;
using WPFApp.ViewModels.Commands;

namespace WPFApp.Rows
{
    internal class BucketRow : Row<Command, BucketRow>
    {
        private FormatString formatString;

        internal BucketRow() : base(true)
        {
            PreviewCommand = new ButtonCommand(_ => TryPreview(), Settings.Default, nameof(Settings.OfflineMode), isOffline => !(bool)isOffline);
            SetPropertyDependency(nameof(CommandString), nameof(Output));
            SetPropertyDependency(nameof(AsString), nameof(FormatString));
        }

        public static event Func<Command, Task> OnCommandPreviewRequested;

        public ICommand PreviewCommand { get; }

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

        protected override SaveResult trySaveChanges() => (FormatString is null || Output is null) ? (new(false)) : FormatString.TryGetPropertiesFromFormatString(Output, true);

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
            FormatString.InputPropertiesFromCommmand(Output);
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
                        PropertyString.Create<PlayCommand>(true, nameof(PlayCommand.Item)),
                        PropertyString.Create<PlayCommand>(false, "at ", nameof(PlayCommand.At)));

                case SeekToCommand:
                    return FormatString.Create(PropertyString.Create<SeekToCommand>(true, nameof(SeekToCommand.At)));

                case SetPlaybackOptionsCommand:
                    return FormatString.Create(
                        PropertyString.Create<SetPlaybackOptionsCommand>(false, "Repeat Mode: ", nameof(SetPlaybackOptionsCommand.RepeatMode)),
                        PropertyString.Create<SetPlaybackOptionsCommand>(false, "Shuffle: ", nameof(SetPlaybackOptionsCommand.Shuffle)),
                        PropertyString.Create<SetPlaybackOptionsCommand>(false, new IntWrapper(0, 100), "Volume %: ", nameof(SetPlaybackOptionsCommand.VolumePercent)));

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

        private void TryPreview()
        {
            if (trySaveChanges())
            {
                _ = OnCommandPreviewRequested?.Invoke(Output);
            }
        }
    }
}