using System;

namespace Utils.Properties
{
    public interface ISetProperty<T> : ISafeProperty<T>
    {
        event PropertySetEventHandler<T> OnSet;
    }

    public interface ISetProperty<T1, T2>
    {
        event PropertySetEventHandler<T1, T2> OnSet;
    }

    public class ReadOnlySetProperty<T> : ISetProperty<T>
    {
        private readonly SetProperty<T> mutable;

        internal ReadOnlySetProperty(SetProperty<T> mutable) => this.mutable = mutable;

        public event PropertySetEventHandler<T> OnSet
        {
            add => mutable.OnSet += value;
            remove => mutable.OnSet -= value;
        }

        public T Get() => mutable.Get();
    }

    public class ReadOnlySetProperty<T1, T2> : ISetProperty<T1, T2>
    {
        private readonly SetProperty<T1, T2> mutable;

        internal ReadOnlySetProperty(SetProperty<T1, T2> mutable) => this.mutable = mutable;

        public event PropertySetEventHandler<T1, T2> OnSet
        {
            add => mutable.OnSet += value;
            remove => mutable.OnSet -= value;
        }

        public T1 Get() => mutable.Get();
    }

    public class SetProperty<T> : SafePropertyBase<T>, ISetProperty<T>
    {
        public SetProperty()
        { }

        public SetProperty(T fieldValue) : base(fieldValue)
        {
        }

        public event PropertySetEventHandler<T> OnSet;

        public event PropertyTrySetEventHandler<T> OnTrySet;

        public ReadOnlySetProperty<T> AsReadOnly => new ReadOnlySetProperty<T>(this);

        public static implicit operator ReadOnlySetProperty<T>(SetProperty<T> setProperty) => setProperty.AsReadOnly;

        public virtual T Set(T value)
        {
            T oldValue = fieldValue;

            if (!(OnTrySet?.Invoke(oldValue, value) ?? true))
            {
                throw new InvalidOperationException();
            }

            fieldValue = value;
            OnSet?.Invoke(oldValue, value);
            return oldValue;
        }
    }

    public class SetProperty<T1, T2> : SafePropertyBase<T1>, ISetProperty<T1, T2>
    {
        public SetProperty()
        { }

        public SetProperty(T1 fieldValue) : base(fieldValue)
        {
        }

        public event PropertySetEventHandler<T1, T2> OnSet;

        public event PropertyTrySetEventHandler<T1, T2> OnTrySet;

        public ReadOnlySetProperty<T1, T2> AsReadOnly => new ReadOnlySetProperty<T1, T2>(this);

        public static implicit operator ReadOnlySetProperty<T1, T2>(SetProperty<T1, T2> setProperty) => setProperty.AsReadOnly;

        public virtual T1 Set(T1 value, T2 extraParam)
        {
            T1 oldValue = fieldValue;

            if (!(OnTrySet?.Invoke(oldValue, value, extraParam) ?? true))
            {
                throw new InvalidOperationException();
            }

            fieldValue = value;
            OnSet?.Invoke(oldValue, value, extraParam);
            return oldValue;
        }
    }
}