using Patterns;
using System;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;
using Utils.Reflection;

namespace WPFApp.Controls.PatternControls
{
	/// <summary>
	/// Interaction logic for NotPatternControl.xaml
	/// </summary>
	public partial class NotPatternControl : UserControl
	{
		public NotPatternControl(NavigationContext navigationContext, Type valueType)
		{
			InitializeComponent();
			PatternPickerWrapper = (IControlWrapper)typeof(SinglePatternPickerWrapper<>).MakeGenericType(valueType).Construct(navigationContext);
			patternPickerContainer.Content = PatternPickerWrapper.UIElement;
			PatternPickerWrapper.ValueSet += NotifyValueChanged;
			NotifyValueChanged();
		}

		public event Action ValueChanged;

		internal IControlWrapper PatternPickerWrapper { get; }

		public SaveResult<IPattern> TryGetPattern(bool trySave)
		{
			var result = PatternPickerWrapper.TryGetObject(trySave);
			return SaveResult.Create<IPattern>(result);
		}

		private void NotifyValueChanged(object _ = null) => ValueChanged?.Invoke();
	}
}