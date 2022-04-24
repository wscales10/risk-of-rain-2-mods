using Rules.RuleTypes.Mutable;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WPFApp.Controls.RuleControls
{
	[ContentProperty(nameof(ContentControl))]
	/// <summary>
	/// Interaction logic for RuleControlWrapper.xaml
	/// </summary>
	public partial class RuleControlWrapper : RuleControlBase
	{
		private RuleControlBase contentControl;

		public RuleControlWrapper(RuleControlBase contentControl) : base(contentControl.NavigationContext)
		{
			InitializeComponent();
			ContentControl = contentControl;
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

		protected override bool ShouldAllowExit()
		{
			string text = ruleNameTextBox.Text;
			ContentControl.Item.Name = text?.Length > 0 ? text : null;
			return ContentControl.TrySave();
		}
	}
}