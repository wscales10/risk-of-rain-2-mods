using SpotifyControlWinForms.Properties;
using SpotifyControlWinForms.Units;

namespace SpotifyControlWinForms
{
	public partial class UnitControl : UserControl
	{
		private readonly UnitBase unit;

		public UnitControl(UnitBase unit)
		{
			this.unit = unit;
			InitializeComponent();
			checkBox.Text = unit.Name;
			checkBox.Checked = unit.IsEnabled;
			unit.IsEnabledChanged += (source, newValue) =>
			{
				if (source != this)
				{
					checkBox.Checked = newValue;
				}
			};
			unit.IsEnabledTogglabilityUpdated += Unit_IsEnabledTogglabilityUpdated;
			Unit_IsEnabledTogglabilityUpdated();
		}

		private void Unit_IsEnabledTogglabilityUpdated()
		{
			checkBox.Enabled = unit.CanToggleIsEnabled;
		}

		private void browseButton_Click(object? sender, EventArgs e)
		{
			var openFileDialog = new OpenFileDialog() { FileName = locationLabel.Text, Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*" };

			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				SaveRuleLocation(locationLabel.Text = openFileDialog.FileName);
			}
		}

		private void SaveRuleLocation(string? location)
		{
			SpotifyControl.SetLocation(unit, location);
			Settings.Default.Save();
			unit.SetRule(location);
		}

		private void checkBox_CheckedChanged(object sender, EventArgs e)
		{
			unit.SetIsEnabled(this, checkBox.Checked);
		}
	}
}