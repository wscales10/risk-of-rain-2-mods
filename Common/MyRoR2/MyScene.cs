using System;
using Utils;

namespace MyRoR2
{
	public class MyScene : IEquatable<MyScene>
	{
		public MyScene(string name) => Name = name;

		public string Name { get; }

		public static bool operator ==(MyScene s1, MyScene s2) => s1 is null ? s2 is null : s1.Equals(s2);

		public static bool operator !=(MyScene s1, MyScene s2) => !(s1 == s2);

		public override string ToString() => HelperMethods.GetNullSafeString(Name);

		public override bool Equals(object obj) => Equals(obj as MyScene);

		public virtual bool Equals(MyScene s)
		{
			if (s is null)
			{
				return false;
			}

			if (ReferenceEquals(this, s))
			{
				return true;
			}

			if (GetType() != s.GetType())
			{
				return false;
			}

			return Name == s.Name;
		}

		public override int GetHashCode() => throw new System.NotImplementedException();
	}
}