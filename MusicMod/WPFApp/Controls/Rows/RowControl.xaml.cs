﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
    /// <summary>
    /// Interaction logic for RowControl.xaml
    /// </summary>
    public partial class RowControl : UserControl
    {
        public RowControl() => InitializeComponent();

        public object HelperContent
        {
            get => GetValue(HelperContentProperty);
            set => SetValue(HelperContentProperty, value);
        }

        public static readonly DependencyProperty HelperContentProperty = DependencyProperty.Register
        (
            nameof(HelperContent),
            typeof(object),
            typeof(RowControl),
            new PropertyMetadata(null)
        );

        public RowViewModelBase ViewModel => (RowViewModelBase)DataContext;

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = (IRow)((FrameworkElement)sender).DataContext;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftShift))
            {
                ViewModel.JointSelect(row);
            }
            else
            {
                ViewModel.Select(row);
            }
        }
    }
}