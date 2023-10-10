using System;
using System.Collections.Generic;
using Patterns;
using Patterns.Patterns;
using Patterns.Patterns.SmallPatterns;
using Utils;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;
using Patterns.TypeDefs;
using System.Linq;
using System.Collections;
using Patterns.Patterns.CollectionPatterns;
using System.Collections.ObjectModel;

namespace WPFApp.Controls.Pickers
{
    public class PatternPickerInfo : IPickerInfo
    {
        private readonly ObservableCollection<Named<object>> patternTypes = new();

        private readonly PropertyWrapper<Type> valueType;

        public PatternPickerInfo(PropertyWrapper<Type> valueType, NavigationContext navigationContext)
        {
            this.valueType = valueType;
            NavigationContext = navigationContext;
            RefreshPatternTypes();
            this.valueType.PropertyChanged += (s, e) => RefreshPatternTypes();
        }

        public PatternPickerInfo(Type valueType, NavigationContext navigationContext)
        {
            this.valueType = new(valueType);
            NavigationContext = navigationContext;
            RefreshPatternTypes();
        }

        public string DisplayMemberPath => nameof(Named<object>.Name);

        public string SelectedValuePath => nameof(Named<object>.Value);

        public NavigationContext NavigationContext { get; }

        public IEnumerable GetItems() => new ReadOnlyObservableCollection<Named<object>>(patternTypes);

        public IReadableControlWrapper CreateWrapper(object selectedInfo)
        {
            switch (selectedInfo)
            {
                case Type selectedType:
                    return PatternWrapper.Create(selectedType, NavigationContext);

                case "paste":
                    var item = NavigationContext.GetClipboardItem();

                    if (item is null)
                    {
                        return null;
                    }
                    else
                    {
                        try
                        {
                            return PatternWrapper.Create(Info.PatternParser.Parse(valueType, item), NavigationContext);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private static List<TypeWrapper> GetAllowedPatternTypes(Type type)
        {
            List<TypeWrapper> output = new();

            if (type.IsGenericType(typeof(Nullable<>)))
            {
                output.AddRange(GetAllowedPatternTypes(type.GenericTypeArguments[0]).Where(w => typeof(IPattern<>).MakeGenericType(type).IsAssignableFrom(w.Type)));
                output.Add(typeof(NullableNullPattern<>).MakeGenericType(type.GenericTypeArguments));
            }
            else
            {
                if (type.IsGenericType(typeof(IList<>)))
                {
                    output.Add(typeof(AnyPattern<>).MakeGenericType(type.GenericTypeArguments));
                    output.Add(typeof(AllPattern<>).MakeGenericType(type.GenericTypeArguments));
                }

                if (Info.PatternParser.TryGetTypeDef(new TypeRef(type), out TypeDef typeDef))
                {
                    Type patternType = typeDef.PatternTypeGetter(type);

                    if (!patternType.IsAbstract)
                    {
                        output.Add(patternType);
                    }
                }

                if (typeof(IComparable).IsAssignableFrom(type) && type.IsValueType)
                {
                    output.Add(typeof(MathsPattern<>).MakeGenericType(type));
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

        private void RefreshPatternTypes()
        {
            patternTypes.ReplaceRange(0, GetAllowedPatternTypes(valueType).Select(w => (Named<object>)w).With(new Named<object>("Paste...", "paste")), patternTypes.Count);
        }
    }
}