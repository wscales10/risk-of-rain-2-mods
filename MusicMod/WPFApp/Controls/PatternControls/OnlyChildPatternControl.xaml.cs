using Patterns;
using System;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;
using Utils.Reflection;
using WPFApp.Controls.Wrappers.SaveResults;
using System.Windows;

namespace WPFApp.Controls.PatternControls
{
    /// <summary>
    /// Interaction logic for OnlyChildPatternControl.xaml
    /// </summary>
    public partial class OnlyChildPatternControl : UserControl
    {
        public static readonly DependencyProperty ParentNameProperty = DependencyProperty.Register
        (
            nameof(ParentName),
            typeof(string),
            typeof(OnlyChildPatternControl),
            new PropertyMetadata(null)
        );

        public OnlyChildPatternControl(NavigationContext navigationContext, Type valueType)
        {
            InitializeComponent();
            PickerWrapper = (IControlWrapper)typeof(SinglePatternPickerWrapper<>).MakeGenericType(valueType).Construct(navigationContext);
            patternPickerContainer.Content = PickerWrapper.UIElement;
            PickerWrapper.ValueSet += NotifyValueChanged;
            NotifyValueChanged();
        }

        public event Action ValueChanged;

        public string ParentName
        {
            get => (string)GetValue(ParentNameProperty);
            set => SetValue(ParentNameProperty, value);
        }

        internal IControlWrapper PickerWrapper { get; }

        public SaveResult<IPattern> TryGetPattern(bool trySave)
        {
            var result = PickerWrapper.TryGetObject(trySave);
            return SaveResult.Create<IPattern>(result);
        }

        private void NotifyValueChanged(object _ = null) => ValueChanged?.Invoke();
    }
}