using Patterns;
using Patterns.Patterns;
using Patterns.Patterns.SmallPatterns;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Utils;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;
using Patterns.TypeDefs;
using System.Linq;

namespace WPFApp.Controls.PatternControls
{
	public abstract class PatternPicker : UserControl
	{
		protected PatternPicker(Type valueType, NavigationContext navigationContext, bool autoCommit = true)
		{
			Init();
			NavigationContext = navigationContext;
			ItemsControl.ItemsSource = GetAllowedPatternTypes(ValueType = valueType);
			ItemsControl.DisplayMemberPath = nameof(TypeWrapper.DisplayName);
			ItemsControl.SelectedValuePath = nameof(TypeWrapper.Type);
			if (autoCommit)
			{
				ItemsControl.SelectionChanged += (s, e) => CommitSelection();
			}
		}

		public Type ValueType { get; }

		public NavigationContext NavigationContext { get; }

		protected abstract Selector ItemsControl { get; }

		public void AddPattern(IPattern pattern) => HandleSelection(PatternWrapper.Create(pattern, NavigationContext));

		protected abstract void Init();

		protected abstract void HandleSelection(IReadableControlWrapper patternWrapper);

		protected void CommitSelection()
		{
			if (ItemsControl.SelectedItem is not null)
			{
				HandleSelection(PatternWrapper.Create((Type)ItemsControl.SelectedValue, NavigationContext));
			}
		}

		private static List<TypeWrapper> GetAllowedPatternTypes(Type type)
		{
			List<TypeWrapper> output = new();

			if (type.IsGenericType(typeof(Nullable<>)))
			{
				output.AddRange(GetAllowedPatternTypes(type.GenericTypeArguments[0]).Where(w => typeof(IPattern<>).MakeGenericType(type).IsAssignableFrom(w.Type)));
				output.Add(typeof(NullableNullPattern<>).MakeGenericType(type.GenericTypeArguments[0]));
			}
			else
			{
				if (Info.PatternParser.TryGetTypeDef(type, out TypeDef typeDef))
				{
					Type patternType = typeDef.PatternTypeGetter(type);
					output.Add(patternType);
				}

				if (type.IsClass)
				{
					output.Add(typeof(ClassNullPattern<>).MakeGenericType(type));
				}

				if (type.GetProperties().Length > 0)
				{
					output.Add(typeof(PropertyPattern<>).MakeGenericType(type));
				}
			}

			output.Add(typeof(OrPattern<>).MakeGenericType(type));
			output.Add(typeof(AndPattern<>).MakeGenericType(type));
			output.Add(typeof(NotPattern<>).MakeGenericType(type));

			return output;
		}

		private class TypeWrapper
		{
			public TypeWrapper(Type type) => Type = type;

			public Type Type { get; }

			public string DisplayName => Type.GetDisplayName();

			public static implicit operator TypeWrapper(Type type) => new(type);
		}
	}
}