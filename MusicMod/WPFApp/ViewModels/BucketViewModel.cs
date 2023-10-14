using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using System.Collections.Specialized;
using WPFApp.Controls.CommandControls;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using Utils;
using System.Linq;
using Spotify.Commands;

namespace WPFApp.ViewModels
{
    internal abstract class BucketViewModel<TContext, TOut> : RuleViewModelBase<Bucket<TContext, TOut>>
    {
        protected BucketViewModel(Bucket<TContext, TOut> rule, NavigationContext navigationContext) : base(rule, navigationContext)
        {
        }

        protected static Bucket<TContext, TOut> FillBucket(Bucket<TContext, TOut> bucket)
        {
            if (bucket.Output is null)
            {
                bucket.Output = Info.Instantiate<TOut>();
            }

            return bucket;
        }
    }

    internal class CommandListBucketViewModel<TContext> : BucketViewModel<TContext, ICommandList>
    {
        public CommandListBucketViewModel(Bucket<TContext, ICommandList> item, NavigationContext navigationContext, PlaylistsController playlistsController) : base(FillBucket(item), navigationContext)
        {
            ((INotifyCollectionChanged)TypedRowManager.Items).CollectionChanged += BucketViewModel_CollectionChanged;
            PropertyString.PlaylistsController = PlaylistsController = playlistsController;
            TypedRowManager.BindTo(Item.Output, AddCommand, r => r.Output);

            ExtraCommands = new[]
            {
                new ButtonContext { Label = "Add Command", Command = new ButtonCommand(_ => AddCommand()) }
            };

            SetPropertyDependency(nameof(AsString), TypedRowManager, nameof(TypedRowManager.Items));
        }

        public override string Title => "Execute in order:";

        public PlaylistsController PlaylistsController { get; }

        public override IEnumerable<ButtonContext> ExtraCommands { get; }

        public override string AsString
        {
            get
            {
                string output = base.AsString;

                if (output is not null)
                {
                    return output;
                }

                int count = TypedRowManager.Items.Count;

                if (count == 0)
                {
                    return null;
                }

                output = TypedRowManager.Items[0].AsString;

                if (count > 1)
                {
                    output += $" (+{count - 1})";
                }

                return output;
            }
        }

        protected override RowManager<CommandListRow> TypedRowManager { get; } = new();

        private void BucketViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged.GetChangedIndices<CommandListRow>(sender, e).Contains(0))
            {
                var oldValues = CollectionChanged.GetOldValues<CommandListRow>(sender, e);

                if (oldValues.Count > 0)
                {
                    RemovePropertyDependency(nameof(AsString), oldValues[0], nameof(CommandListRow.AsString));
                }

                if (TypedRowManager.Items.Count > 0)
                {
                    SetPropertyDependency(nameof(AsString), TypedRowManager.Items[0], nameof(CommandListRow.AsString));
                }
            }
        }

        private CommandListRow AddCommand(Command command = default)
        {
            PropertyString.NavigationContext = NavigationContext;
            return TypedRowManager.Add(new CommandListRow(NavigationContext) { Output = command });
        }
    }
}