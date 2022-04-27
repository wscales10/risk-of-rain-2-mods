using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Linq;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.RuleControls
{
	[ContentProperty(nameof(ContentControl))]
	public partial class RuleControlWrapper : ControlBase, IXmlControl
	{
		private RuleControlBase contentControl;

		public RuleControlWrapper(RuleControlBase contentControl) : base(contentControl.NavigationContext)
		{
			InitializeComponent();
			ContentControl = contentControl;
		}

		public event Action OnItemChanged
		{
			add
			{
				((IItemControl)ContentControl).OnItemChanged += value;
			}

			remove
			{
				((IItemControl)ContentControl).OnItemChanged -= value;
			}
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

		public Rule Item => ContentControl.Item;

		public string ItemTypeName => ContentControl.ItemTypeName;

		public object ItemObject => ContentControl.ItemObject;

		public XElement GetContentXml() => ContentControl.GetContentXml();

		protected override SaveResult ShouldAllowExit()
		{
			string text = ruleNameTextBox.Text;
			ContentControl.Item.Name = text?.Length > 0 ? text : null;
			return ContentControl.TrySave();
		}
	}
}