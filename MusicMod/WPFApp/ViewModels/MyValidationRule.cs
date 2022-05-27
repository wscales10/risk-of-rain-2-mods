using System;

namespace WPFApp.ViewModels
{
    public class MyValidationRule<T> : IValidationRule
    {
        public MyValidationRule(Predicate<T> validate, Func<T, string> getErrorMessage)
        {
            Validate = validate;
            GetErrorMessage = getErrorMessage;
        }

        public Predicate<T> Validate { get; }

        public Func<T, string> GetErrorMessage { get; }

        string IValidationRule.GetErrorMessage(object obj) => obj is T value ? GetErrorMessage(value) : "Incorrect type";

        bool IValidationRule.Validate(object obj) => obj is T value && Validate(value);
    }
}