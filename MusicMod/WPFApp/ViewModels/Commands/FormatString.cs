using Spotify.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Utils;
using Utils.Reflection.Properties;
using WPFApp.Controls;
using WPFApp.SaveResults;

namespace WPFApp.ViewModels.Commands
{
    internal sealed class FormatString : NotifyDataErrorInfoBase
    {
        private readonly ObservableCollection<IPropertyString> notDisplayed = new();

        internal FormatString(Type type, params IPropertyString[] propertyStrings)
        {
            Type = type;
            PropertyStrings = propertyStrings.ToReadOnlyCollection();
            FilteredPropertyStrings = CollectionViewSource.GetDefaultView(PropertyStrings);
            FilteredPropertyStrings.Filter = item => !notDisplayed.Contains(item);

            foreach (IPropertyString propertyString in PropertyStrings)
            {
                SetPropertyDependency(nameof(AsString), propertyString, nameof(IPropertyString.AsString));
            }

            notDisplayed.CollectionChanged += (s, e) =>
            {
                FilteredPropertyStrings.Refresh();
                NotifyPropertyChanged(nameof(AsString));
            };

            MenuViewModel = new(notDisplayed);
            MenuViewModel.OnValueSelected += Display;
        }

        public ICollectionView FilteredPropertyStrings { get; }

        public DiscreteMenuViewModel<IPropertyString> MenuViewModel { get; }

        public ReadOnlyCollection<IPropertyString> PropertyStrings { get; }

        public string AsString => string.Join(" ", FilteredPropertyStrings.Cast<IPropertyString>().Select(ps => ps.AsString));

        public Type Type { get; }

        internal static FormatString Create<T>(params PropertyString<T>[] propertyStrings) => new(typeof(T), propertyStrings);

        internal void BuildControl()
        {
            foreach (IPropertyString propertyString in PropertyStrings)
            {
                Hide(propertyString);

                if (propertyString.IsRequired)
                {
                    Display(propertyString);
                }
                else
                {
                    propertyString.FocusElement.KeyDown += (s, e) =>
                    {
                        switch (e.Key)
                        {
                            case Key.Delete:
                                Hide(propertyString);
                                ShiftFocus(-1);
                                break;

                            case Key.Left:
                                ShiftFocus(-1);
                                return;

                            case Key.Right:
                                ShiftFocus(1);
                                return;
                        }

                        e.Handled = true;

                        void ShiftFocus(int diff)
                        {
                            int index = PropertyStrings.IndexOf(propertyString);
                            IPropertyString neighbour;

                            do
                            {
                                if (index == (diff > 0 ? PropertyStrings.Count - 1 : 0))
                                {
                                    return;
                                }

                                index += diff;
                                neighbour = PropertyStrings[index];
                            } while (notDisplayed.Contains(neighbour));

                            e.Handled = neighbour.FocusElement.Focus();
                        }
                    };
                }
            }
        }

        internal void InputPropertiesFromCommmand(Command command)
        {
            foreach (var propertyString in PropertyStrings)
            {
                object value = command.GetPropertyValue(propertyString.PropertyName);

                if (propertyString.IsRequired || value is not null)
                {
                    propertyString.ValueObject = value;
                    Display(propertyString);
                }
                else
                {
                    Hide(propertyString);
                }
            }
        }

        internal SaveResult TryGetPropertiesFromFormatString(Command command, bool trySave)
        {
            return PropertyStrings.All(propertyString =>
            {
                if (!propertyString.IsRequired && notDisplayed.Contains(propertyString))
                {
                    command.SetPropertyValue(propertyString.PropertyName, null);
                    return null;
                }
                else
                {
                    if (propertyString.HasErrors)
                    {
                        return new SaveResult<object>(false);
                    }
                    else
                    {
                        command.SetPropertyValue(propertyString.PropertyName, propertyString.ValueObject);
                        return new SaveResult<object>(true, propertyString.ValueObject);
                    }
                }
            });
        }

        private void Display(IPropertyString propertyString)
        {
            if (notDisplayed.Contains(propertyString))
            {
                _ = notDisplayed.Remove(propertyString);
            }
        }

        private void Hide(IPropertyString propertyString)
        {
            if (!notDisplayed.Contains(propertyString))
            {
                notDisplayed.Add(propertyString);
            }
        }
    }
}