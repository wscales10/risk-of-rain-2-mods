using Patterns;
using Spotify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Utils;
using Utils.Reflection;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;
using System.Reflection;
using System.Linq;
using Spotify.Commands;

namespace WPFApp.Controls.CommandControls
{
    internal abstract class PropertyString : NotifyPropertyChangedBase
    {
        private readonly Type propertyType;

        private readonly StackPanel stackPanel = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };

        private readonly ArrayList stringParts = new();

        private bool isInitialised;

        private IControlWrapper controlWrapper;

        protected PropertyString(Type objectType, bool required, IControlWrapper controlWrapper, string[] chunks)
        {
            MakeUi();

            foreach (string chunk in chunks)
            {
                PropertyInfo propertyInfo = objectType.GetProperty(chunk);

                if (propertyInfo is null)
                {
                    stackPanel.Children.Add(GetLabel(chunk));
                    stringParts.Add(chunk);
                }
                else
                {
                    PropertyName = chunk;
                    propertyType = propertyInfo.PropertyType;
                    this.controlWrapper = controlWrapper;
                }
            }

            if (IsRequired = required)
            {
                Initialise();
            }
        }

        public string AsString => string.Concat(GetStringParts());

        public UIElement UI { get; private set; }

        public Border FocusElement { get; } = new Border { Focusable = true, FocusVisualStyle = null };

        public IControlWrapper ControlWrapper
        {
            get
            {
                Initialise();
                return controlWrapper;
            }
        }

        public string PropertyName { get; }

        public bool IsRequired { get; }

        internal static NavigationContext NavigationContext { get; set; }

        private static ReadOnlyDictionary<Type, Func<IControlWrapper>> Controls { get; } = new(new Dictionary<Type, Func<IControlWrapper>>
        {
            [typeof(TimeSpan)] = () => new TimeSpanWrapper(),
            [typeof(ISpotifyItem)] = () => new SpotifyItemWrapper2(NavigationContext),
            [typeof(SpotifyItem)] = () => new SpotifyItemWrapper(),
            [typeof(bool)] = () => new BoolWrapper(),
            [typeof(IOffset)] = () => new OffsetWrapper(NavigationContext)
        });

        public static PropertyString<T> Create<T>(bool required, params string[] chunks) => new(required, chunks);

        public static PropertyString<T> Create<T>(bool required, IControlWrapper controlWrapper, params string[] chunks) => new(required, controlWrapper, chunks);

        internal void Initialise()
        {
            if (!isInitialised)
            {
                controlWrapper ??= GetControlWrapper(propertyType);
                SetPropertyDependency(nameof(AsString), controlWrapper, nameof(controlWrapper.ValueString));

                if (!IsRequired && propertyType.IsGenericType(typeof(Nullable<>)))
                {
                    controlWrapper.SetValue(Activator.CreateInstance(propertyType.GenericTypeArguments[0]));
                }

                controlWrapper.UIElement.Margin = new Thickness(1);
                stackPanel.Children.Add(controlWrapper.UIElement);
                stringParts.Add(controlWrapper);
                isInitialised = true;
            }
        }

        private static IControlWrapper GetControlWrapper(Type type)
        {
            var valueType = type.IsGenericType(typeof(Nullable<>)) ? type.GenericTypeArguments[0] : type;

            if (typeof(Enum).IsAssignableFrom(valueType))
            {
                return EnumWrapper.Create(valueType);
            }

            if (valueType.IsGenericType(typeof(IPattern<>)))
            {
                return (IControlWrapper)typeof(SinglePatternPickerWrapper<>).MakeGenericType(valueType.GenericTypeArguments).Construct(NavigationContext);
            }

            if (valueType.IsGenericType(typeof(Switch<,>)))
            {
                throw new InvalidOperationException("You have to specify this in the constructor call");
            }

            if (!Controls.TryGetValue(type, out var func) && !Controls.TryGetValue(valueType, out func))
            {
                throw new NotImplementedException();
            }

            return func();
        }

        private IEnumerable<string> GetStringParts()
        {
            return stringParts.Cast<object>().Select(GetStringPart);

            static string GetStringPart(object obj) => obj switch
            {
                string s => s,
                IControlWrapper wrapper => wrapper.ValueString,
                _ => throw new ArgumentOutOfRangeException(nameof(obj)),
            };
        }

        private void MakeUi()
        {
            var output = new Grid { Margin = new Thickness(1) };
            _ = output.Children.Add(FocusElement);
            FocusElement.Child = new Rectangle()
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 0.5,
                StrokeDashArray = new(new double[] { 1, 2 }),
                SnapsToDevicePixels = true,
                Visibility = Visibility.Hidden,
            };

            FocusElement.GotFocus += (s, e) => FocusElement.Child.Visibility = Visibility.Visible;
            FocusElement.LostFocus += (s, e) => FocusElement.Child.Visibility = Visibility.Hidden;

            _ = output.Children.Add(stackPanel);
            UI = output;
        }

        private Label GetLabel(string text)
        {
            Label label = new()
            {
                Content = text,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0),
                Padding = new Thickness()
            };

            label.PreviewMouseLeftButtonDown += (s, e) => e.Handled = FocusElement.Focus();
            return label;
        }
    }

    internal class PropertyString<TObject> : PropertyString
    {
        public PropertyString(bool required, params string[] chunks) : this(required, null, chunks)
        {
        }

        public PropertyString(bool required, IControlWrapper controlWrapper, params string[] chunks) : base(typeof(TObject), required, controlWrapper, chunks)
        {
        }
    }
}