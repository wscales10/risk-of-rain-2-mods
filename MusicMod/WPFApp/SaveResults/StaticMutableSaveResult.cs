using System;
using System.ComponentModel;

namespace WPFApp.SaveResults
{
    public static class MutableSaveResult
    {
        //public static DynamicMutableSaveResult<T> Create<T>(Func<T> getter, Action<T> setter) => new(getter, setter);
    }

    public abstract class MutableSaveResultBase<TValue> : NotifyPropertyChangedBase, ISaveResult<TValue>
    {
        private bool? status;

        protected MutableSaveResultBase()
        {
            SetPropertyDependency(nameof(IsSuccess), nameof(Status));
            PropertyChanged += MutableSaveResultBase_PropertyChanged;
        }

        public event MyValidationEventHandler2<TValue> OnValidate;

        public bool IsSuccess => Status ?? true;

        public bool? Status { get => status; private set => SetProperty(ref status, value); }

        public abstract TValue Value { get; set; }

        public void MaybeOutput(Action<TValue> setter)
        {
            if (IsSuccess)
            {
                setter(Value);
            }
        }

        private void MutableSaveResultBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Value))
            {
                MyValidationEventArgs2<TValue> args = new(Value);
                OnValidate?.Invoke(this, args);
                Status = args.Status;
            }
        }
    }

    public class StaticMutableSaveResult<TValue> : MutableSaveResultBase<TValue>
    {
        private TValue value;

        public override TValue Value
        {
            get => value;

            set => SetProperty(ref this.value, value);
        }
    }

    //public class DynamicMutableSaveResult<TValue> : MutableSaveResultBase<TValue>
    //{
    //    private readonly Func<TValue> getter;

    // private readonly Action<TValue> setter;

    // public DynamicMutableSaveResult(Func<TValue> getter, Action<TValue> setter) { this.getter =
    // getter; this.setter = setter; }

    // public override TValue Value { get => getter();

    //        set
    //        {
    //            setter(value);
    //            NotifyPropertyChanged();
    //        }
    //    }
    //}
}