using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Utils;
using Utils.Reflection.Properties;

namespace WPFApp.ViewModels
{
    public abstract class NotifyDataErrorInfoBase : NotifyPropertyChangedBase, INotifyDataErrorInfo
    {
        private static readonly Dictionary<Type, Func<IValidationRule>> defaultValidationRules = new();

        private readonly AutoInitialiseDictionary<string, List<string>> errorsByPropertyName = new(list => list.Count == 0);

        private readonly AutoInitialiseDictionary<string, List<IValidationRule>> validationDictionary = new(list => list.Count == 0);

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => errorsByPropertyName.Count > 0;

        public static IValidationRule GetDefaultValidationRule(Type type) => defaultValidationRules[type]();

        public IEnumerable GetErrors(string propertyName)
        {
            return errorsByPropertyName.ContainsKey(propertyName) ?
                errorsByPropertyName[propertyName] : null;
        }

        protected sealed override void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            ValidateProperty(propertyName);
            base.NotifyPropertyChanged(propertyName);
        }

        protected void DefinePropertyValidation(string propertyName, IValidationRule validationRule = null)
        {
            validationDictionary[propertyName].Add(validationRule ?? GetDefaultValidationRule(GetType().GetProperty(propertyName).PropertyType));
        }

        protected void ValidateProperty([CallerMemberName] string propertyName = null)
        {
            ClearErrors(propertyName);

            var value = this.GetPropertyValue(propertyName);

            if (validationDictionary.ContainsKey(propertyName))
            {
                foreach (var rule in validationDictionary[propertyName])
                {
                    if (!rule.Validate(value))
                    {
                        AddError(propertyName, rule.GetErrorMessage(value));
                    }
                }
            }
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void AddError(string propertyName, string error)
        {
            if (!errorsByPropertyName[propertyName].Contains(error))
            {
                errorsByPropertyName[propertyName].Add(error);
                OnErrorsChanged(propertyName);
            }
        }

        private void ClearErrors(string propertyName)
        {
            if (errorsByPropertyName.ContainsKey(propertyName))
            {
                errorsByPropertyName[propertyName].Clear();
                errorsByPropertyName.TryRemove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }
    }
}