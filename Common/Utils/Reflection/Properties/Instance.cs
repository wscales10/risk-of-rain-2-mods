namespace Utils.Reflection.Properties
{
	public static class Instance
	{
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
	}
}