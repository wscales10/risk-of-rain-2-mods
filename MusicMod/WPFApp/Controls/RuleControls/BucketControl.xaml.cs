using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.CommandControls;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.RuleControls
{
	/// <summary>
	/// Interaction logic for BucketControl.xaml
	/// </summary>
	public partial class BucketControl : RuleControlBase, IRowControl
	{
		private readonly RowManager<BucketRow> rowManager;

		public BucketControl(Bucket rule, NavigationContext ctx) : base(ctx)
		{
			Item = rule;
			InitializeComponent();
			newCommandTypeComboBox.ItemsSource = Info.SupportedCommandTypes;
			newCommandTypeComboBox.DisplayMemberPath = nameof(Type.Name);
			newCommandTypeComboBox.SelectedIndex = 0;

			rowManager = new(commandsGrid);
			rowManager.BindTo(Item.Commands, AddCommand, r => r.Output);

			rowButtonsControl.BindTo(rowManager);
		}

		IRowManager IRowControl.RowManager => rowManager;

		public override Bucket Item { get; }

		protected override SaveResult ShouldAllowExit() => rowManager.TrySaveChanges();

		private static void Shift(LinkedListNode<BucketRow> node, bool down)
		{
			if (node is null)
			{
				return;
			}

			if (down)
			{
				Shift(node.Next, true);
			}

			foreach (UIElement element in node.Value.Elements)
			{
				Grid.SetRow(element, Grid.GetRow(element) + (down ? 1 : -1));
			}

			if (!down)
			{
				Shift(node.Next, false);
			}
		}

		private BucketRow AddCommand(Command command)
		{
			if (command is null)
			{
				return null;
			}

			PropertyString.NavigationContext = NavigationContext;
			return rowManager.Add(new BucketRow(command));
		}

		private void AddCommandButton_Click(object sender, RoutedEventArgs e)
		{
			Command command = ((Type)newCommandTypeComboBox.SelectedItem).Name switch
			{
				nameof(PauseCommand) => new PauseCommand(),
				nameof(ResumeCommand) => new ResumeCommand(),
				nameof(StopCommand) => new StopCommand(),
				nameof(PlayCommand) => new PlayCommand(),
				nameof(SeekToCommand) => SeekToCommand.AtMilliseconds(0),
				nameof(SetPlaybackOptionsCommand) => new SetPlaybackOptionsCommand(),
				nameof(TransferCommand) => new TransferCommand(),
				_ => null,
			};

			AddCommand(command);
		}
	}
}