using HtmlAgilityPack;
using MyRoR2;
using Patterns;
using Patterns.Patterns;
using Patterns.Patterns.SmallPatterns;
using System;
using System.Windows;
using Utils;
using Utils.Reflection;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	public delegate HtmlWeb HtmlWebRequestHandler();

	internal static class PatternWrapper
	{
		internal static event HtmlWebRequestHandler OnHtmlWebRequested;

		public static IReadableControlWrapper Create(IPattern pattern, NavigationContext navigationContext) => Create(pattern.GetType(), pattern, navigationContext);

		public static IReadableControlWrapper Create(Type patternType, NavigationContext navigationContext)
		{
			var pattern = (IPattern)patternType.Construct();
			return Create(patternType, pattern, navigationContext);
		}

		internal static HtmlWeb RequestHtmlWeb() => OnHtmlWebRequested?.Invoke();

		private static IReadableControlWrapper Create(Type patternType, IPattern pattern, NavigationContext navigationContext)
		{
			if (typeof(IListPattern).IsAssignableFrom(patternType))
			{
				return new ListPatternWrapper(pattern as IListPattern, navigationContext);
			}

			if (patternType.IsGenericType(typeof(NotPattern<>)))
			{
				return (IReadableControlWrapper)typeof(NotPatternWrapper<>).MakeGenericType(patternType.GenericTypeArguments).Construct(pattern, navigationContext);
			}

			if (patternType == typeof(ScenePattern))
			{
				return new ScenePatternWrapper(pattern as ScenePattern);
			}

			if (patternType == typeof(EntityPattern))
			{
				return new EntityPatternWrapper(pattern as EntityPattern);
			}

			if (patternType.IsGenericType(typeof(PropertyPattern<>)))
			{
				return (IReadableControlWrapper)typeof(PropertyPatternWrapper<>).MakeGenericType(patternType.GenericTypeArguments).GetAnyConstructor(patternType, typeof(NavigationContext)).Invoke(new object[] { pattern, navigationContext });
			}

			if (typeof(IValuePattern).IsAssignableFrom(patternType))
			{
				return Construct(typeof(ValuePatternWrapper<>).MakeGenericType(patternType));
			}

			if (patternType.IsGenericType(typeof(ClassNullPattern<>)))
			{
				return Construct(typeof(ClassNullPatternWrapper<>).MakeGenericType(patternType.GenericTypeArguments));
			}

			if (patternType.IsGenericType(typeof(NullableNullPattern<>)))
			{
				return Construct(typeof(NullableNullPatternWrapper<>).MakeGenericType(patternType.GenericTypeArguments));
			}

			return new TestPatternWrapper(pattern);

			IReadableControlWrapper Construct(Type wrapperType)
			{
				return (IReadableControlWrapper)wrapperType.GetAnyConstructor(patternType).Invoke(new[] { pattern });
			}
		}
	}

	internal abstract class PatternWrapper<TPattern, TControl> : ControlWrapper<TPattern, TControl>
			where TPattern : IPattern
		where TControl : FrameworkElement
	{
	}
}