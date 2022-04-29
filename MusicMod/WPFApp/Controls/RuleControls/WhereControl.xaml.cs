﻿using MyRoR2;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;

namespace WPFApp.Controls.RuleControls
{
	/// <summary>
	/// Interaction logic for WhereControl.xaml
	/// </summary>
	public partial class WhereControl : UserControl
	{
		public WhereControl() => InitializeComponent();

		internal NavigationContext NavigationContext
		{
			set
			{
				PatternWrapper = new OptionalPatternPickerWrapper<Context>(value);
				container.Child = PatternWrapper.UIElement;
			}
		}

		internal IControlWrapper PatternWrapper { get; private set; }
	}
}