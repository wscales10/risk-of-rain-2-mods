﻿using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using System.Collections.Generic;
using System.Collections.Specialized;
using WPFApp.Controls.CommandControls;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using Utils;
using System.Linq;

namespace WPFApp.ViewModels
{
	internal class BucketViewModel : RuleViewModelBase<Bucket>
	{
		public BucketViewModel(Bucket item, NavigationContext navigationContext) : base(item, navigationContext)
		{
			TypedRowManager.BindTo(Item.Commands, AddCommand, r => r.Output);

			ExtraCommands = new[]
			{
				new ButtonContext { Label = "Add Command", Command = new ButtonCommand(_ => AddCommand()) }
			};

			SetPropertyDependency(nameof(AsString), TypedRowManager, nameof(TypedRowManager.Items));

			((INotifyCollectionChanged)TypedRowManager.Items).CollectionChanged += BucketViewModel_CollectionChanged;
		}

		public override IEnumerable<ButtonContext> ExtraCommands { get; }

		public string AsString
		{
			get
			{
				int count = TypedRowManager.Items.Count;

				if (count == 0)
				{
					return null;
				}

				string output = TypedRowManager.Items[0].FormatString.AsString;

				if (count > 1)
				{
					output += $" (+{count - 1})";
				}

				return output;
			}
		}

		protected override RowManager<BucketRow> TypedRowManager { get; } = new();

		private void BucketViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged.GetChangedIndices<BucketRow>(sender, e).Contains(0))
			{
				RemovePropertyDependency(nameof(AsString), CollectionChanged.GetOldValues<BucketRow>(sender, e)[0].FormatString, nameof(FormatString.AsString));
				SetPropertyDependency(nameof(AsString), TypedRowManager.Items[0].FormatString, nameof(FormatString.AsString));
			}
		}

		private BucketRow AddCommand(Command command = null)
		{
			PropertyString.NavigationContext = NavigationContext;
			return TypedRowManager.Add(new BucketRow(command));
		}
	}
}