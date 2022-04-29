using System.Collections.Generic;
using System.Linq;
using WPFApp.Controls;
using Rules.RuleTypes.Mutable;
using System;
using Microsoft.Win32;
using System.Windows;

namespace WPFApp.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private ControlBase control;

		private bool hasContent;

		private bool isXmlControl;

		private TreeNode masterNode;

		private bool hasContent2;

		public MainViewModel(NavigationContext navigationContext)
		{
			NavigationContext = navigationContext;
			NavigateTreeCommand = new(action);
			BackCommand = new(_ => OnGoBack?.Invoke(), false);
			ForwardCommand = new(_ => OnGoForward?.Invoke(), false);
			ImportCommand = new(_ =>
			{
				OpenFileDialog dialog = new() { Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*" };

				if (dialog.ShowDialog() == true)
				{
					OnImportFile?.Invoke(dialog.FileName);
				}
			});
			ExportCommand = new(_ =>
			{
				if (TryGetExportLocation(out string fileName))
				{
					OnExportFile?.Invoke(fileName);
				}
			});
			NewCommand = new(_ =>
			{
				if (MessageBox.Show("Are you sure you want to close this?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
				{
					OnReset?.Invoke();
				}
			}, this, nameof(HasContent));
			HomeCommand = new(_ => NavigationContext.GoHome(), NavigationContext, nameof(NavigationContext.IsHome), b => !(bool)b);
			UpCommand = new(_ => NavigationContext.GoUp(), NavigationContext, nameof(NavigationContext.IsHome), b => !(bool)b);
		}

		public event Action OnReset;

		public event Action OnGoBack;

		public event Action OnGoForward;

		public event Action<string> OnImportFile;

		public event Action<string> OnExportFile;

		public ButtonCommand NavigateTreeCommand { get; }

		public ButtonCommand BackCommand { get; }

		public ButtonCommand ForwardCommand { get; }

		public ButtonCommand UpCommand { get; }

		public ButtonCommand HomeCommand { get; }

		public ButtonCommand ImportCommand { get; }

		public ButtonCommand ExportCommand { get; }

		public ButtonCommand NewCommand { get; }

		public NavigationContext NavigationContext { get; }

		public ControlBase Control
		{
			get => control;

			set
			{
				control = value;
				HasContent = value is not null;
				ExportCommand.CanExecute = value is IXmlControl;
				NotifyPropertyChanged();
			}
		}

		public bool HasContent
		{
			get => hasContent;

			set
			{
				hasContent = value;
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

		private static bool TryGetExportLocation(out string fileName)
		{
			SaveFileDialog dialog = new() { Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*" };

			if (dialog.ShowDialog() == true)
			{
				fileName = dialog.FileName;
				return true;
			}
			else
			{
				fileName = null;
				return false;
			}
		}

		private void action(object treeNodeObject)
		{
			var node = CurrentNode;
			int count = 0;
			while ((node = node.Parent) is not null)
			{
				count++;
			}

			node = (TreeNode)treeNodeObject;
			List<TreeNode> nodes = new();
			while (node.Parent is not null)
			{
				nodes.Add(node);
				node = node.Parent;
			}

			nodes.Reverse();

			NavigationContext.GoUp(count);
			_ = NavigationContext.GoInto(nodes.Select(n => n.Value as Rule));
		}
	}
}