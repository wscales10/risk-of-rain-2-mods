using Patterns;
using Patterns.Patterns;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;
using System.Windows;
using Utils.Properties;
using Utils.Reflection;

namespace WPFApp.Controls.PatternControls
{
    /// <summary>
    /// Interaction logic for PropertyPatternControl.xaml
    /// </summary>
    public partial class PropertyPatternControl : UserControl
    {
        private readonly Dictionary<string, IControlWrapper> patternPickerWrappers = new();

        private readonly SetProperty<IControlWrapper> PickerWrapper = new();

        private Type objectType;

        public PropertyPatternControl()
        {
            InitializeComponent();
            PickerWrapper.OnSet += propertySetEventHandler;
            propertyComboBox.SelectionChanged += (s, e) => ValueChanged?.Invoke();
        }

        public PropertyPatternControl(NavigationContext navigationContext) : this() => NavigationContext = navigationContext;

        public PropertyPatternControl(Type objectType, NavigationContext navigationContext) : this(navigationContext) => ObjectType = objectType;

        public PropertyPatternControl(Type objectType) : this() => ObjectType = objectType;

        public event Action ValueChanged;

        public IPattern WaitingPattern { private get; set; }

        public NavigationContext NavigationContext { get; set; }

        public Type ObjectType
        {
            get => objectType;

            set
            {
                propertyComboBox.ItemsSource = Info.GetProperties(value);
                objectType = value;
            }
        }

        public PropertyInfo SelectedProperty
        {
            get => (PropertyInfo)propertyComboBox.SelectedItem;

            set
            {
                if ((PropertyInfo)propertyComboBox.SelectedItem == value)
                {
                    propertyComboBox_SelectionChanged(this, null);
                }
                else
                {
                    propertyComboBox.SelectedValue = value?.Name;
                }
            }
        }

        public SaveResult<IPattern> TryGetPattern(bool trySave)
        {
            var result = PickerWrapper.Get().TryGetObject(trySave);
            return SaveResult.Create<IPattern>(result);
        }

        private void propertySetEventHandler(IControlWrapper oldValue, IControlWrapper value)
        {
            if (oldValue is not null)
            {
                oldValue.ValueSet -= NotifyValueChanged;
            }

            patternPickerContainer.Child = value?.UIElement;
            patternPickerLabel.Visibility = value is null ? Visibility.Hidden : Visibility.Visible;

            if (value is not null)
            {
                value.ValueSet += NotifyValueChanged;
            }

            NotifyValueChanged();
        }

        private void NotifyValueChanged(object _ = null) => ValueChanged?.Invoke();

        private void propertyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (PropertyInfo)propertyComboBox.SelectedItem;

            if (selectedItem is null)
            {
                PickerWrapper.Set(null);
            }
            else if (patternPickerWrappers.TryGetValue(selectedItem.Name, out IControlWrapper value))
            {
                PickerWrapper.Set(value);
            }
            else
            {
                PickerWrapper.Set((IControlWrapper)typeof(SinglePatternPickerWrapper<>).MakeGenericType(selectedItem.Type).Construct(NavigationContext));
                patternPickerWrappers[selectedItem.Name] = PickerWrapper.Get();
            }

            if (WaitingPattern is not null)
            {
                PickerWrapper.Get().SetValue(WaitingPattern);
                WaitingPattern = null;
            }
        }
    }
}