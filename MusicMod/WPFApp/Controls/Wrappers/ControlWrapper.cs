using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFApp.Controls.Wrappers
{
    public interface IWrapper
    {
        FrameworkElement UIElement { get; }
    }

    public interface IReadableControlWrapper : IWrapper, INotifyPropertyChanged
    {
        event Action<bool?> StatusSet;

        event Action<object> ValueSet;

        string ValueString { get; }

        SaveResult<object> TryGetObject(bool trySave);

        void ForceGetValue(out object value);

        void Focus();
    }

    internal interface IReadableControlWrapper<TValue> : IReadableControlWrapper
    {
        SaveResult<TValue> TryGetValue(bool trySave);

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

    public abstract class ReadableControlWrapper<TValue, TControl> : NotifyPropertyChangedBase, IReadableControlWrapper<TValue>
        where TControl : FrameworkElement
    {
        private bool wantsFocus;

        private bool isInitialised;

        protected ReadableControlWrapper()
        {
            Init();
            valueSet += ReadableControlWrapper_ValueSet;
            ValueCleared += ReadableControlWrapper_ValueCleared;
        }

        public event Action<bool?> StatusSet;

        public event Action<object> ValueSet;

        public event Action ValueStringChanged;

        protected event Action ValueCleared;

        protected event Action<TValue> valueSet;

        public abstract TControl UIElement { get; }

        FrameworkElement IWrapper.UIElement => UIElement;

        public virtual UIElement FocusElement => UIElement;

        public virtual string ValueString => tryGetValue(false)?.Value?.ToString();

        protected bool HighlightStatus { get; private set; }

        public void ForceGetValue(out object value)
        {
            ForceGetValue(out TValue x);
            value = x;
        }

        public void ForceGetValue(out TValue value)
        {
            var result = tryGetValue(false);
            if (!result.IsSuccess)
            {
                throw new NotSupportedException();
            }

            value = result.Value;
        }

        public SaveResult<TValue> TryGetValue(bool trySave)
        {
            if (trySave)
            {
                HighlightStatus = true;
            }

            var result = tryGetValue(trySave);
            result &= Validate(result.Value);

            if (trySave)
            {
                result.ReleaseActions.Enqueue(() => StopHighlighting());
                SetStatus(result.Status);
            }

            return result;
        }

        public SaveResult<object> TryGetObject(bool trySave)
        {
            var result = TryGetValue(trySave);
            return new(result, result.Value);
        }

        public void Focus()
        {
            if (!isInitialised)
            {
                FocusElement.IsVisibleChanged += (s, e) =>
                {
                    if ((bool)e.NewValue && wantsFocus)
                    {
                        _ = ((UIElement)s).Focus();
                        wantsFocus = false;
                    }
                };

                isInitialised = true;
            }

            wantsFocus = true;
        }

        protected static void Outline(Control control, bool status)
        {
            control.BorderBrush = status switch
            {
                false => Brushes.Red,
                _ => Brushes.DarkGray,
            };
        }

        protected static void Outline(Border border, bool status)
        {
            border.BorderBrush = status switch
            {
                false => Brushes.Red,
                _ => Brushes.DarkGray,
            };
        }

        protected static void Outline(Control control, bool? status, bool threeWay = false)
        {
            control.BorderBrush = status switch
            {
                true when threeWay => Brushes.Green,
                false => Brushes.Red,
                _ => Brushes.DarkGray,
            };
        }

        protected virtual void ReadableControlWrapper_ValueSet(TValue value)
        {
            if (Validate(value))
            {
                SetStatus(true);
            }

            ValueSet?.Invoke(value);
        }

        protected void NotifyValueChanged()
        {
            var result = tryGetValue(false);

            if (result.IsSuccess)
            {
                NotifyValueChanged(result.Value);
            }
            else
            {
                ValueCleared?.Invoke();
            }

            NotifyPropertyChanged(nameof(ValueString));
        }

        protected void NotifyValueChanged(TValue value) => valueSet?.Invoke(value);

        protected abstract SaveResult<TValue> tryGetValue(bool trySave);

        protected virtual void setStatus(bool? status) => setStatus(status ?? true);

        protected virtual void setStatus(bool status)
        {
            if (UIElement is Control c)
            {
                Outline(c, (bool?)status);
            }
        }

        protected virtual bool Validate(TValue value) => true;

        protected virtual void ReadableControlWrapper_ValueCleared()
        {
            SetStatus(null);
            ValueSet?.Invoke(null);
        }

        protected virtual void StopHighlighting(bool clearStatus = true)
        {
            HighlightStatus = false;
            if (clearStatus)
            {
                SetStatus(null);
            }
        }

        protected virtual void Init()
        {
        }

        private void SetStatus(bool? status)
        {
            if (HighlightStatus)
            {
                setStatus(status);
                StatusSet?.Invoke(status);
            }
        }
    }
}