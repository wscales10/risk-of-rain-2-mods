using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using WPFApp.Controls.PatternControls;
using WPFApp.Controls.Wrappers.PatternWrappers;

namespace WPFApp.Controls.RuleControls
{
	/// <summary>
	/// Interaction logic for CaseControl.xaml
	/// </summary>
	public partial class CaseControl : UserControl
	{
		private readonly NavigationContext navigationContext;

		private readonly MultiPatternPicker patternPicker;

		public CaseControl(Case<IPattern> c, Type valueType, NavigationContext navigationContext)
		{
			InitializeComponent();
			this.navigationContext = navigationContext;
			patternPicker = new(valueType, navigationContext);
			patternPickerContainer.Child = patternPicker;
			patternPicker.PatternContainerManager.BindTo(c.Arr, AddPattern, c =>
			{
				c.PatternWrapper.ForceGetValue(out object value);
				return (IPattern)value;
			});
		}

		public bool TrySaveChanges() => patternPicker.TrySaveChanges();

		private PatternContainer AddPattern(IPattern pattern) => new(PatternWrapper.Create(pattern, navigationContext), patternPicker.PatternContainerManager.Add);
	}
}