using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using System.Collections.Generic;
using System.Collections.Specialized;
using WPFApp.Controls.GridManagers;
using WPFApp.Rows;
using Utils;
using System.Linq;
using WPFApp.ViewModels.Commands;

namespace WPFApp.ViewModels
{
    internal class BucketViewModel : RuleViewModelBase<Bucket>
    {
        public BucketViewModel(Bucket item, NavigationContext navigationContext) : base(item, navigationContext)
        {
            ((INotifyCollectionChanged)TypedRowManager.Items).CollectionChanged += BucketViewModel_CollectionChanged;
            TypedRowManager.BindTo(Item.Commands, AddCommand, r => r.Output);

            ExtraCommands = new[]
            {
                new ButtonContext { Label = "Add Command", Command = new ButtonCommand(_ => AddCommand()) }
            };

            SetPropertyDependency(nameof(AsString), TypedRowManager, nameof(TypedRowManager.Items));
        }

        public override string Title => "Execute in order:";

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

        protected override RowManager<BucketRow> TypedRowManager { get; } = new();

        private void BucketViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged.GetChangedIndices<BucketRow>(sender, e).Contains(0))
            {
                var oldValues = CollectionChanged.GetOldValues<BucketRow>(sender, e);

                if (oldValues.Count > 0)
                {
                    RemovePropertyDependency(nameof(AsString), oldValues[0], nameof(BucketRow.AsString));
                }

                if (TypedRowManager.Items.Count > 0)
                {
                    SetPropertyDependency(nameof(AsString), TypedRowManager.Items[0], nameof(BucketRow.AsString));
                }
            }
        }

        private BucketRow AddCommand(Command command = null)
        {
            PropertyString.NavigationContext = NavigationContext;
            return TypedRowManager.Add(new BucketRow() { Output = command });
        }
    }
}