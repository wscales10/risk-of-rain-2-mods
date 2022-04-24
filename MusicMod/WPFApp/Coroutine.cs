using System;
using System.Collections;

namespace WPFApp
{
	public class Reference
	{
		private readonly PrivateReference privateReference;

		internal Reference(PrivateReference privateReference) => this.privateReference = privateReference;

		public object Value
		{
			get => privateReference.Value;
			set => privateReference.Value = value;
		}

		public void Cancel() => privateReference.Cancel = true;
	}

	public class PrivateReference
	{
		public object Value { get; set; }

		public bool Cancel { get; set; }

		public Reference MakePublic() => new(this);
	}

	public class Coroutine
	{
		private readonly Func<Func<Reference, IEnumerable>> getter;

		public Coroutine(Func<Func<Reference, IEnumerable>> getter) => this.getter = getter;

		public bool Invoke(Action<object, Reference> handler)
		{
			Func<Reference, IEnumerable> func = getter();

			if (func is null)
			{
				return false;
			}

			var reference = new PrivateReference();
			Reference publicReference = reference.MakePublic();

			foreach (object result in func(publicReference))
			{
				if (reference.Cancel)
				{
					return false;
				}
				reference.Value = null;
				handler(result, publicReference);
				if (reference.Cancel)
				{
					return false;
				}
			}

			return true;
		}
	}
}