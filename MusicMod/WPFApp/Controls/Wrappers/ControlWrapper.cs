using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    public interface IWrapper
    {
        FrameworkElement UIElement { get; }

        bool NeedsRightMargin { get; }

        bool NeedsLeftMargin { get; }
    }

    public interface IReadableControlWrapper : IWrapper, INotifyPropertyChanged
    {
        event Action<bool?> StatusSet;

        event Action<object> ValueSet;

        string ValueString { get; }

        SaveResult<object> TryGetObject(GetValueRequest request);

        void ForceGetValue(out object value);

        void Focus();
    }

    internal interface IReadableControlWrapper<TValue> : IReadableControlWrapper
    {
        event MyValidationEventHandler<TValue> OnValidate;

        SaveResult<TValue> TryGetValue(GetValueRequest request);

        void ForceGetValue(out TValue value);
    }

    internal interface IControlWrapper : IReadableControlWrapper
    {
        void SetValue(object value);
    }

    internal interface IControlWrapper<TValue> : IReadableControlWrapper<TValue>
    {
        void SetValue(TValue value);
    }

    public abstract class ControlWrapper<TValue, TControl> : ReadableControlWrapper<TValue, TControl>, IControlWrapper, IControlWrapper<TValue>
        where TControl : FrameworkElement
    {
        private Func<TValue, bool> validator;

        public void SetValue(TValue value)
        {
            setValue(value);
            StopHighlighting();
            NotifyValueChanged();
        }

        public void SetValue(object value)
        {
            if (value is not null and not TValue)
            {
                Debugger.Break();
            }

            var typed = (TValue)value;
            SetValue(typed);
        }

        public ControlWrapper<TValue, TControl> WithValidation(Func<TValue, bool> validator)
        {
            if (this.validator is not null)
            {
                throw new NotImplementedException();
            }

            this.validator = validator;
            return this;
        }

        protected abstract void setValue(TValue value);

        protected override bool Validate(TValue value)
        {
            return validator is null ? base.Validate(value) : validator(value);
        }
    }
}