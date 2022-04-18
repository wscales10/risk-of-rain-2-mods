using System.Linq;
using System.Windows.Controls;
using WPFApp.Controls.GridManagers;
using System.Collections.Generic;

namespace WPFApp.Controls.Rows
{
	/// <summary>
	/// Interaction logic for RowButtonsControl.xaml
	/// </summary>
	public partial class RowButtonsControl : UserControl
	{
		public RowButtonsControl() => InitializeComponent();

		internal void BindTo(IRowManager rowManager)
		{
			void UpdateButtons()
			{
				UpButton.IsEnabled = rowManager.CanMoveSelected(false);
				DownButton.IsEnabled = rowManager.CanMoveSelected(true);
				DeleteButton.IsEnabled = rowManager.CanRemoveSelected();
			}

			rowManager.SelectionChanged += UpdateButtons;

			UpButton.Click += (s, e) => rowManager.MoveSelected(false);
			DownButton.Click += (s, e) => rowManager.MoveSelected(true);
			DeleteButton.Click += (s, e) => rowManager.RemoveSelected();

			UpdateButtons();
		}
	}
}