using System;

namespace Utils.Coroutines
{
	public class InputRequest<T>
	{
		private bool isSet;

		private T input;

		public InputRequest(string name = null) => Name = name;

		public string Name { get; }

		public T Input
		{
			get => input; set

			{
				if (isSet)
				{
					throw new InvalidOperationException("cannot set input twice");
				}

				input = value;
				isSet = true;
			}
		}
	}
}