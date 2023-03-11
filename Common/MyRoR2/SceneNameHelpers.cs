using Patterns;
using Patterns.Patterns;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;

namespace MyRoR2
{
	public delegate bool Try<T>(out T variable);

	public static class SceneNameHelpers
	{
		public static Fase<T> F<T>(Func<T> getter, params DefinedScene[] constants)
		{
			return new Fase<T>((out T variable) => { variable = getter(); return true; }, constants);
		}

		public static Fase<T> F<T>(T value, params DefinedScene[] constants)
		{
			return new Fase<T>((out T variable) => { variable = value; return true; }, constants);
		}

		public static Fase<T> F<T>(Try<T> setter, params DefinedScene[] constants)
		{
			return new Fase<T>(setter, constants);
		}

		public static Fase<T> F<T>(T value, Func<bool> condition, params DefinedScene[] constants)
		{
			return new Fase<T>((out T variable) => Compress(out variable, condition, value), constants);
		}

		public static bool GetValueFromSceneName<T>(out T variable, MyScene scene, params Fase<T>[] fases)
		{
			foreach (var fase in fases)
			{
				if (fase.Candidates.Any(c => ScenePattern.Equals(c).IsMatch(scene)))
				{
					if (fase.TrySet(out variable))
					{
						return true;
					}
				}
			}

			variable = default;
			return false;
		}

		public static Pattern<MyScene> GetPattern(params DefinedScene[] candidates)
		{
			return new OrPattern<MyScene>(candidates.Select(c => ScenePattern.Equals(c)));
		}

		public static PropertyPattern<RoR2Context> GetPropertyPattern(params DefinedScene[] candidates)
		{
			return Query<RoR2Context>.Create(nameof(RoR2Context.SceneName), GetPattern(candidates));
		}

		private static bool Compress<T>(out T variable, Func<bool> condition, T value)
		{
			if (condition())
			{
				variable = value;
				return true;
			}

			variable = default;
			return false;
		}
	}

	public class Fase<T>
	{
		public Fase(Try<T> @try, params DefinedScene[] candidates)
		{
			TrySet = @try;
			Candidates = candidates.ToReadOnlyCollection();
		}

		public ReadOnlyCollection<DefinedScene> Candidates { get; }

		public Try<T> TrySet { get; }
	}
}