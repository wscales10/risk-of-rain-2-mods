using System;
using Utils;

namespace MyRoR2
{
	public class Entity : IEquatable<Entity>
	{
		public Entity(string name) => Name = name;

		public string Name { get; }

		public static bool operator ==(Entity e1, Entity e2) => e1 is null ? e2 is null : e1.Equals(e2);

		public static bool operator !=(Entity e1, Entity e2) => !(e1 == e2);

		public override string ToString() => HelperMethods.GetNullSafeString(Name);

		public override bool Equals(object obj) => Equals(obj as Entity);

		public virtual bool Equals(Entity e)
		{
			if (e is null)
			{
				return false;
			}

			if (ReferenceEquals(this, e))
			{
				return true;
			}

			if (GetType() != e.GetType())
			{
				return false;
			}

			return Name == e.Name;
		}

		public override int GetHashCode() => Name.GetHashCode();
	}
}