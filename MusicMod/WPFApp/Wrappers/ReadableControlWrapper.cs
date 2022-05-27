using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPFApp.Wrappers.SaveResults;

namespace WPFApp.Wrappers
{
    // TODO: use OnValidate
    public abstract class ReadableControlWrapper<TValue, TControl> : NotifyPropertyChangedBase, IReadableControlWrapper<TValue>
        where TControl : FrameworkElement
    {
        private bool wantsFocus;

        private bool isInitialised;

        private bool? status;

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

        public bool? Status
        {
            get => status;

            private set
            {
                SetProperty(ref status, value);
                StatusSet?.Invoke(value);
            }
        }

        protected bool HighlightStatus { get; private set; }

        public void ForceGetValue(out object value)
        {
            ForceGetValue(out TValue x);
            value = x;
        }

        public void ForceGetValue(out TValue value)
        {
            var result = tryGetValue2(new(false, true));
            if (!result.IsSuccess)
            {
                throw new NotSupportedException();
            }

            value = result.Value;
        }

        public SaveResult<TValue> TryGetValue(GetValueRequest request)
        {
            if (request.TrySave)
            {
                HighlightStatus = true;
            }

            var result = tryGetValue2(request);

            if (!request.BypassValidation)
            {
                result &= Validate(result.Value);
            }

            if (request.TrySave)
            {
                result.ReleaseActions.Enqueue(() => StopHighlighting());
                MaybeSetStatus(result.Status);
            }

            return result;
        }

        public SaveResult<object> TryGetObject(GetValueRequest request)
        {
            var result = TryGetValue(request);
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

        protected static void Outline(Control control, bool status, Brush trueBrush = null, Brush falseBrush = null)
        {
            control.BorderBrush = GetOutlineColour(status, trueBrush, falseBrush);
        }

        protected static void Outline(Border border, bool status, Brush trueBrush = null, Brush falseBrush = null)
        {
            border.BorderBrush = GetOutlineColour(status, trueBrush, falseBrush);
        }

        protected static void Outline(Control control, bool? status, bool threeWay = false, Brush trueBrush = null, Brush falseBrush = null, Brush nullBrush = null)
        {
            control.BorderBrush = GetOutlineColour(status, threeWay, trueBrush, falseBrush, nullBrush);
        }

        protected static void Outline(Border border, bool? status, bool threeWay = false, Brush trueBrush = null, Brush falseBrush = null, Brush nullBrush = null)
        {
            border.BorderBrush = GetOutlineColour(status, threeWay, trueBrush, falseBrush, nullBrush);
        }

        protected static Brush GetOutlineColour(bool status, Brush trueBrush = null, Brush falseBrush = null) => status switch
        {
            false => falseBrush ?? Brushes.Red,
            _ => trueBrush ?? Brushes.DarkGray,
        };

        protected static Brush GetOutlineColour(bool? status, bool threeWay = false, Brush trueBrush = null, Brush falseBrush = null, Brush nullBrush = null)
        {
            if (threeWay)
            {
                return status switch
                {
                    true => trueBrush ?? Brushes.Green,
                    false => falseBrush ?? Brushes.Red,
                    _ => nullBrush ?? Brushes.DarkGray,
                };
            }
            else
            {
                if (trueBrush is not null && nullBrush is not null)
                {
                    throw new InvalidOperationException();
                }

                return status switch
                {
                    false => falseBrush ?? Brushes.Red,
                    _ => trueBrush ?? nullBrush ?? Brushes.DarkGray,
                };
            }
        }

        protected virtual void ReadableControlWrapper_ValueSet(TValue value)
        {
            if (Validate(value))
            {
                MaybeSetStatus(true);
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

        protected abstract SaveResult<TValue> tryGetValue(GetValueRequest request);

        protected virtual bool Validate(TValue value) => true;

        protected virtual void ReadableControlWrapper_ValueCleared()
        {
            MaybeSetStatus(null);
            ValueSet?.Invoke(null);
        }

        protected virtual void StopHighlighting(bool clearStatus = true)
        {
            HighlightStatus = false;
            if (clearStatus)
            {
                Status = null;
            }
        }

        private SaveResult<TValue> tryGetValue2(GetValueRequest request)
        {
            var output = tryGetValue(request);
            var args = new MyValidationEventArgs<TValue>(output);
            OnValidate?.Invoke(this, args);
            return args.SaveResult;
        }

        private void MaybeSetStatus(bool? status)
        {
            if (HighlightStatus)
            {
                Status = status;
            }
        }
    }
}