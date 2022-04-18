﻿using System.Windows;

namespace WPFApp.Views
{
	/// <summary>
	/// Interaction logic for ControlTestView.xaml
	/// </summary>
	public partial class ControlTestView : Window
	{
		public ControlTestView()
		{
			InitializeComponent();
			box.ItemsSource = new[] { 1, 2, 3 };
		}
	}
}
