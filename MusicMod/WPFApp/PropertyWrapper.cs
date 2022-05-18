using System.ComponentModel;

namespace WPFApp
{
    public class PropertyWrapper<T> : INotifyPropertyChanged
    {
        private T value;

        public PropertyWrapper(T value) => this.value = value;

        public PropertyWrapper()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public T Value
        {
            get => value;

            set
            {
                this.value = value;
                PropertyChanged?.Invoke(this, new(nameof(Value)));
            }
        }

        public static implicit operator T(PropertyWrapper<T> propertyWrapper) => propertyWrapper.Value;

        public override string ToString() => Value?.ToString();
    }
}