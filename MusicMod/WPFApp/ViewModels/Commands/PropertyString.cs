using Patterns;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Utils;
using Utils.Reflection;
using System.Reflection;
using System.Linq;
using System.Text;

namespace WPFApp.ViewModels.Commands
{
    internal static class PropertyString
    {
        internal static NavigationContext NavigationContext { get; set; }

        public static PropertyString<T> Create<T>(bool required, params string[] chunks) => new(required, chunks);

        public static PropertyString<T> Create<T>(bool required, IControlWrapper controlWrapper, params string[] chunks) => new(required, controlWrapper, chunks);
    }

    internal sealed class PropertyString<T> : NotifyDataErrorInfoBase, IPropertyString
    {
        private T value;

        public PropertyString(bool required, params string[] chunks) : this(required, null, chunks)
        {
        }

        public PropertyString(bool required, IControlWrapper controlWrapper, params string[] chunks) : this(typeof(T), required, controlWrapper, chunks)
        {
        }

        private PropertyString(Type objectType, bool required, IControlWrapper controlWrapper, string[] chunks)
        {
            bool movedOn = false;
            StringBuilder prefixBuilder = new();
            StringBuilder suffixBuilder = new();

            foreach (string chunk in chunks)
            {
                PropertyInfo propertyInfo = objectType.GetProperty(chunk);

                if (propertyInfo is null)
                {
                    if (movedOn)
                    {
                        throw new InvalidOperationException("Only one property allowed");
                    }

                    movedOn = true;
                }
                else
                {
                    PropertyName = chunk;
                    ControlWrapper = controlWrapper ?? GetControlWrapper(propertyInfo.PropertyType);
                    SetPropertyDependency(nameof(AsString), ControlWrapper, nameof(ControlWrapper.ValueString));

                    if (!required && propertyInfo.PropertyType.IsGenericType(typeof(Nullable<>)))
                    {
                        ControlWrapper.SetValue(Activator.CreateInstance(propertyInfo.PropertyType.GenericTypeArguments[0]));
                    }

                    if (movedOn)
                    {
                        suffixBuilder.Append(chunk);
                    }
                    else
                    {
                        prefixBuilder.Append(chunk);
                    }
                }
            }

            Prefix = prefixBuilder.ToString();
            Suffix = suffixBuilder.ToString();
            IsRequired = required;
        }

        public string Prefix { get; } = string.Empty;

        public string Suffix { get; } = string.Empty;

        public string AsString => string.Concat(Prefix, Value, Suffix);

        public string PropertyName { get; }

        public bool IsRequired { get; }

        object IPropertyString.ValueObject
        {
            get => Value;
            set => Value = (T)value;
        }

        // TODO: validate?
        public T Value
        {
            get => value;
            set => SetProperty(ref this.value, value);
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
                return (IControlWrapper)typeof(SinglePatternPickerWrapper<>).MakeGenericType(valueType.GenericTypeArguments).Construct(PropertyString.NavigationContext);
            }

            if (valueType.IsGenericType(typeof(Switch<,>)))
            {
                throw new InvalidOperationException("You have to specify this in the constructor call");
            }

            return Controls.TryGetValue(type, out var func) ? func() : Controls[valueType]();
        }
    }
}