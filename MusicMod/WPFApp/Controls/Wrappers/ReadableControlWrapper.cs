﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    // TODO: use OnValidate
    public abstract class ReadableControlWrapper<TValue, TControl> : NotifyPropertyChangedBase, IReadableControlWrapper<TValue>
        where TControl : FrameworkElement
    {
        private bool wantsFocus;

        private bool isInitialised;

        protected ReadableControlWrapper()
        {
            valueSet += ReadableControlWrapper_ValueSet;
            ValueCleared += ReadableControlWrapper_ValueCleared;
        }

        public event MyValidationEventHandler<TValue> OnValidate;

        public event Action<bool?> StatusSet;

        public event Action<object> ValueSet;

        protected event Action ValueCleared;

        protected event Action<TValue> valueSet;

        public abstract TControl UIElement { get; }

        FrameworkElement IWrapper.UIElement => UIElement;

        public virtual UIElement FocusElement => UIElement;

        public virtual string ValueString => tryGetValue2(false)?.Value?.ToString();

        public virtual bool NeedsRightMargin => true;

        public virtual bool NeedsLeftMargin => NeedsRightMargin;

        protected bool HighlightStatus { get; private set; }

        public void ForceGetValue(out object value)
        {
            ForceGetValue(out TValue x);
            value = x;
        }

        public void ForceGetValue(out TValue value)
        {
            var result = tryGetValue2(false);
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

            var result = tryGetValue2(trySave);
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
            var result = tryGetValue2(false);

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

        private SaveResult<TValue> tryGetValue2(bool trySave)
        {
            var output = tryGetValue(trySave);
            var args = new MyValidationEventArgs<TValue>(output);
            OnValidate?.Invoke(this, args);
            return args.SaveResult;
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