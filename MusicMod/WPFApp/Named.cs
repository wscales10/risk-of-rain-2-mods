namespace WPFApp
{
	public class Named<T>
	{
		public Named(string name, T obj)
		{
			Name = name;
			Value = obj;
		}

		public string Name { get; }

		public T Value { get; }
	}
}