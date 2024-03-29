﻿using System.Windows;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Pickers
{
	public class MultiPickerViewModel : PickerViewModel
	{
		public MultiPickerViewModel(IPickerInfo config) : base(config)
		{
		}

		public ValueContainerManager ValueContainerManager { get; } = new();

		internal ValueContainer AddWrapper(IReadableControlWrapper valueWrapper) => ValueContainerManager.Add(new(valueWrapper) { Margin = new Thickness(1) });

		protected override void handleSelection(IReadableControlWrapper valueWrapper) => _ = AddWrapper(valueWrapper);
	}
}