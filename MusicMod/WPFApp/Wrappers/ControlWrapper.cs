using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using WPFApp.Wrappers.SaveResults;

namespace WPFApp.Wrappers
{
    public interface IWrapper
    {
        FrameworkElement UIElement { get; }

        bool? Status { get; }

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

        protected abstract void setValue(TValue value);
    }
}