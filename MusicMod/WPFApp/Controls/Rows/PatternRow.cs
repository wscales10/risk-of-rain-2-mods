using Patterns;
using System;
using System.Windows;
using Utils.Reflection;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;

namespace WPFApp.Controls.Rows
{
	internal class PatternRow : Row<IPattern, PatternRow>
	{
		private readonly IControlWrapper singlePatternPickerWrapper;

		public PatternRow(IPattern pattern, Type valueType, NavigationContext navigationContext) : base(pattern, true)
		{
			singlePatternPickerWrapper = (IControlWrapper)typeof(SinglePatternPickerWrapper<>).MakeGenericType(valueType).Construct(navigationContext);
			singlePatternPickerWrapper.ValueSet += SinglePatternPickerWrapper_ValueSet;
			singlePatternPickerWrapper.SetValue(pattern);
		}

		public override SaveResult TrySaveChanges() => singlePatternPickerWrapper.TryGetObject(true);

		protected override UIElement MakeOutputUi()
		{
			var output = singlePatternPickerWrapper?.UIElement;

			if(output is not null)
			{
				output.Margin = new Thickness(40, 4, 4, 4);
				output.MinWidth = 150;
				output.HorizontalAlignment = HorizontalAlignment.Left;
			}

			return output;
		}

		private void SinglePatternPickerWrapper_ValueSet(object obj) => Output = (IPattern)obj;

		/*		new SinglePatternPicker(valueType, navigationContext)
				{
					Margin = new Thickness(40, 4, 4, 4),
					VerticalAlignment = VerticalAlignment.Center,
					MinWidth = 150,
					HorizontalAlignment = HorizontalAlignment.Left,
				};*/
	}
}