using System.Collections.Generic;
using System.Linq;
using WPFApp.Controls;
using Rules.RuleTypes.Mutable;

namespace WPFApp.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private ControlBase control;

		private bool hasContent;

		private bool isXmlControl;

		private TreeNode masterNode;

		public MainViewModel(NavigationContext navigationContext)
		{
			NavigationContext = navigationContext;
			Command = new(o =>
			{
				var node = CurrentNode;
				int count = 0;
				while ((node = node.Parent) is not null)
				{
					count++;
				}

				node = (TreeNode)o;
				List<TreeNode> nodes = new();
				while (node.Parent is not null)
				{
					nodes.Add(node);
					node = node.Parent;
				}

				nodes.Reverse();

				NavigationContext.GoUp(count);
				_ = NavigationContext.GoInto(nodes.Select(n => n.Value as Rule));
			});
		}

		public ButtonCommand Command { get; }

		public NavigationContext NavigationContext { get; }

		public ControlBase Control
		{
			get => control;

			set
			{
				control = value;
				HasContent = value is not null;
				IsXmlControl = value is IXmlControl;
				NotifyPropertyChanged();
			}
		}

		public bool HasContent
		{
			get => hasContent;

			private set
			{
				hasContent = value;
				NotifyPropertyChanged();
			}
		}

		public bool IsXmlControl
		{
			get => isXmlControl;

			private set
			{
				isXmlControl = value;
				NotifyPropertyChanged();
			}
		}

		public TreeNode MasterNode
		{
			get => masterNode;

			set
			{
				masterNode = value;
				NotifyPropertyChanged();
			}
		}

		public TreeNode CurrentNode { get; set; }
	}
}