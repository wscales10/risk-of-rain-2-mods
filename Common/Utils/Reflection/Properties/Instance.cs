using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

		public static object GetDeepPropertyValue(this object instance, string path)
		{
			var propertyNames = path.Split('.');
			Type objectType = instance.GetType();

			foreach (var propertyName in propertyNames)
			{
				PropertyInfo propInfo = objectType.GetProperty(propertyName);

				if (propInfo != null)
				{
					instance = propInfo.GetValue(instance, null);
					objectType = instance.GetType();
				}
				else
				{
					throw new ArgumentException("Properties path is not correct");
				}
			}

			return instance;
		}

		public static bool PropertywiseEquals<T>(this T item1, T item2)
		{
			if (typeof(T).IsClass)
			{
				if ((object)item1 is null)
				{
					return (object)item2 is null;
				}
			}
			else if (!typeof(T).IsValueType)
			{
				throw new NotSupportedException();
			}

			foreach (var propertyInfo in typeof(T).GetProperties())
			{
				var value1 = propertyInfo.GetValue(item1);
				var value2 = propertyInfo.GetValue(item2);

				if (value1 is null)
				{
					if (!(value2 is null))
					{
						return false;
					}
				}
				else if (!value1.Equals(value2))
				{
					return false;
				}
			}

			return true;
		}

		public static PropertyInfo[] GetPublicProperties(this Type type)
		{
			if (type.IsInterface)
			{
				var propertyInfos = new List<PropertyInfo>();

				var considered = new List<Type>();
				var queue = new Queue<Type>();
				considered.Add(type);
				queue.Enqueue(type);
				while (queue.Count > 0)
				{
					var subType = queue.Dequeue();
					foreach (var subInterface in subType.GetInterfaces())
					{
						if (considered.Contains(subInterface)) continue;

						considered.Add(subInterface);
						queue.Enqueue(subInterface);
					}

					var typeProperties = subType.GetProperties(
						BindingFlags.FlattenHierarchy
						| BindingFlags.Public
						| BindingFlags.Instance);

					var newPropertyInfos = typeProperties
						.Where(x => !propertyInfos.Contains(x));

					propertyInfos.InsertRange(0, newPropertyInfos);
				}

				return propertyInfos.ToArray();
			}

			return type.GetProperties(BindingFlags.FlattenHierarchy
				| BindingFlags.Public | BindingFlags.Instance);
		}
	}
}