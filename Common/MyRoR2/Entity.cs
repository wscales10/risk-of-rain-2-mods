using System;
using System.Collections.Generic;
using Utils;
using Utils.Reflection.Properties;

namespace MyRoR2
{
	public class Entity
	{
		public Entity(string name) => Name = name;

		public string Name { get; }
	}
}