using MathNet.Symbolics;
using Spotify.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPFApp.Controls.CommandControls;
using WPFApp.Controls.Wrappers;
using WPFApp.Converters;
using WPFApp.Properties;

namespace WPFApp.Controls.Rows
{
	internal class BucketRow : Row<Command, BucketRow>
	{
		private FormatString formatString;

		internal BucketRow(Command command) : base(command, true)
		{
			LeftElement.Click += (s, e) =>
			{
				if (TrySaveChanges())
				{
					_ = OnCommandPreviewRequested?.Invoke(command);
				}
			};

			LeftElement.Content = command?.GetType().Name.Replace(nameof(Command), string.Empty);

			Binding binding = new(nameof(Settings.OfflineMode))
			{
				Source = Settings.Default,
				Converter = new InverseBooleanConverter()
			};

			_ = LeftElement.SetBinding(UIElement.IsEnabledProperty, binding);
		}

		public static event Func<Command, Task> OnCommandPreviewRequested;

		public override Button LeftElement { get; } = new()
		{
			VerticalAlignment = VerticalAlignment.Center,
			FontSize = 14,
			Margin = new Thickness(4),
			HorizontalAlignment = HorizontalAlignment.Center
		};

		public override SaveResult TrySaveChanges() => formatString.TryGetProperties(Output, true);

		protected override UIElement MakeOutputUi()
		{
			formatString = GetFormatString(Output);
			var stackPanel = formatString.BuildControl();
			formatString.SetProperties(Output);
			return stackPanel;
		}

		private static FormatString GetFormatString(Command command)
		{
			switch (command)
			{
				case PlayCommand:
					return FormatString.Create(
						PropertyString.Create<PlayCommand>(true, nameof(PlayCommand.Item)),
						PropertyString.Create<PlayCommand>(false, "at", nameof(PlayCommand.At)));

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
						PropertyString.Create<TransferCommand>(true, new SwitchWrapper<int, SymbolicExpression>(TryParse), "at ", nameof(TransferCommand.Mapping))
						);
					static bool TryParse(string s, out SymbolicExpression output)
					{
						try
						{
							output = SymbolicExpression.Parse(s);
						}
						catch (Exception)
						{
							output = null;
							return false;
						}

						try
						{
							_ = TransferCommand.Map(0, output);
						}
						catch (Exception e) when (e.Message.StartsWith("Failed to find symbol"))
						{
							output = null;
							return false;
						}

						return true;
					}
				default:
					return null;
			}
		}
	}
}