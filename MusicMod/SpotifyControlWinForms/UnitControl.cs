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

			locationLabel.Visible = unit is IRuleUnit;
			browseButton.Visible = unit is IRuleUnit;
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
			if (unit is IRuleUnit ruleUnit)
			{
				SpotifyControl.SetLocation(ruleUnit, location);
				Settings.Default.Save();
				ruleUnit.SetRule(location);
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private void checkBox_CheckedChanged(object sender, EventArgs e)
		{
			unit.SetIsEnabled(this, checkBox.Checked);
		}
	}
}