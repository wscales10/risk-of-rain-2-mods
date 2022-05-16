namespace Utils.Reflection.Properties
{
    public static class Instance
    {
        public static bool TryGetPropertyValue(this object obj, string propertyName, out object value)
        {
            var property = obj?.GetType().GetProperty(propertyName);
            value = property?.GetValue(obj);
            return !(property is null);
        }

        public static bool TryGetPropertyValue<T>(this object obj, string propertyName, out T value)
        {
            var property = obj?.GetType().GetProperty(propertyName);

            if (!(property is null))
            {
                object valueObject = property?.GetValue(obj);

                if (valueObject is T typedValue)
                {
                    value = typedValue;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            return obj?.GetType().GetProperty(propertyName).GetValue(obj);
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            return (T)obj?.GetPropertyValue(propertyName);
        }

        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            obj.GetType().GetProperty(propertyName).SetValue(obj, value);
        }

        public static T ConvertToMutable<T>(this object obj)
            where T : new()
        {
            var output = new T();
            foreach (var property in typeof(T).GetProperties())
            {
                if (obj.TryGetPropertyValue(property.Name, out object value))
                {
                    output.SetPropertyValue(property.Name, value);
                }
            }
            return output;
        }
    }
}