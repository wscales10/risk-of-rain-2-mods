using Rules.RuleTypes.Mutable;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Markup;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.RuleControls
{
	[ContentProperty(nameof(ContentControl))]
	public partial class RuleControlWrapper : RuleControlBase
	{
		private RuleControlBase contentControl;

		public RuleControlWrapper(RuleControlBase contentControl) : base(contentControl.NavigationContext)
		{
			InitializeComponent();
			ContentControl = contentControl;
			ContentControl.OnItemChanged += NotifyItemChanged;
		}

		public RuleControlBase ContentControl
		{
			get => contentControl;

			set
			{
				masterGrid.Children.Remove(contentControl);
				contentControl = value;
				Grid.SetRow(contentControl, 1);
				_ = masterGrid.Children.Add(contentControl);
				ruleNameTextBox.Text = contentControl?.Item.Name;
			}
		}

		public override Rule Item => ContentControl.Item;

		public override ReadOnlyObservableCollection<(string, RuleControlBase)> Children => ContentControl.Children;

		public override string ToString() => $"{GetType().Name}({ContentControl})";

		protected override SaveResult ShouldAllowExit()
		{
			string text = ruleNameTextBox.Text;
			ContentControl.Item.Name = text?.Length > 0 ? text : null;
			return ContentControl.TrySave();
		}
	}
}