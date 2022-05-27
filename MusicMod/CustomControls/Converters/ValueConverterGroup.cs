using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace CustomControls.Converters
{
    [ContentProperty("Converters")]
    public class ValueConverterGroup : IValueConverter
    {
        private readonly Dictionary<IValueConverter, ValueConversionAttribute> cachedAttributes = new();

        public ValueConverterGroup() => Converters.CollectionChanged += OnConvertersCollectionChanged;

        public ObservableCollection<IValueConverter> Converters { get; } = new();

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object output = value;

            for (int i = 0; i < Converters.Count; ++i)
            {
                IValueConverter converter = Converters[i];
                Type currentTargetType = GetTargetType(i, targetType, true);
                output = converter.Convert(output, currentTargetType, parameter, culture);

                if (output == Binding.DoNothing)
                {
                    break;
                }
            }

            return output;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object output = value;

            for (int i = Converters.Count - 1; i > -1; --i)
            {
                IValueConverter converter = Converters[i];
                Type currentTargetType = GetTargetType(i, targetType, false);
                output = converter.ConvertBack(output, currentTargetType, parameter, culture);

                if (output == Binding.DoNothing)
                {
                    break;
                }
            }

            return output;
        }

        protected virtual Type GetTargetType(int converterIndex, Type finalTargetType, bool convert)
        {
            IValueConverter nextConverter = null;
            if (convert)
            {
                if (converterIndex < Converters.Count - 1)
                {
                    nextConverter = Converters[converterIndex + 1];
                    if (nextConverter == null)
                    {
                        throw new InvalidOperationException($"The Converters collection of the ValueConverterGroup contains a null reference at index: {converterIndex + 1}");
                    }
                }
            }
            else if (converterIndex > 0)
            {
                nextConverter = Converters[converterIndex - 1];
                if (nextConverter == null)
                {
                    throw new InvalidOperationException($"The Converters collection of the ValueConverterGroup contains a null reference at index: {converterIndex - 1}");
                }
            }

            if (nextConverter != null)
            {
                ValueConversionAttribute conversionAttribute = cachedAttributes[nextConverter];
                return convert ? conversionAttribute.SourceType : conversionAttribute.TargetType;
            }

            return finalTargetType;
        }

        private void OnConvertersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IList convertersToProcess = null;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    convertersToProcess = e.NewItems;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (IValueConverter converter in e.OldItems)
                    {
                        _ = cachedAttributes.Remove(converter);
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    cachedAttributes.Clear();
                    convertersToProcess = Converters;
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (convertersToProcess?.Count > 0)
            {
                foreach (IValueConverter converter in convertersToProcess)
                {
                    object[] attributes = converter.GetType().GetCustomAttributes(typeof(ValueConversionAttribute), false);

                    if (attributes.Length != 1)
                    {
                        throw new InvalidOperationException("All value converters added to a ValueConverterGroup must be decorated with the ValueConversionAttribute attribute exactly once.");
                    }

                    cachedAttributes.Add(converter, attributes[0] as ValueConversionAttribute);
                }
            }
        }
    }
}