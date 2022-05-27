using System;
using System.Collections.Generic;
using Patterns;
using Patterns.Patterns;
using Patterns.Patterns.SmallPatterns;
using Utils;
using Patterns.TypeDefs;
using System.Linq;
using Patterns.Patterns.CollectionPatterns;
using System.Collections.ObjectModel;
using Utils.Reflection;

namespace WPFApp.ViewModels.Pickers
{
    internal class PatternPickerInfo : TypeWrapperPickerInfo
    {
        private readonly ObservableCollection<TypeWrapper> patternTypes = new();

        private readonly PropertyWrapper<Type> valueType;

        public PatternPickerInfo(PropertyWrapper<Type> valueType, NavigationContext navigationContext) : base(navigationContext)
        {
            this.valueType = valueType;
            RefreshPatternTypes();
            this.valueType.PropertyChanged += (s, e) => RefreshPatternTypes();
        }

        public PatternPickerInfo(Type valueType, NavigationContext navigationContext) : base(navigationContext)
        {
            this.valueType = new(valueType);
            RefreshPatternTypes();
        }

        public override IEnumerable<TypeWrapper> GetItems() => new ReadOnlyObservableCollection<TypeWrapper>(patternTypes);

        public override object CreateItem(TypeWrapper selectedType)
        {
            return selectedType.Type.Construct();
        }

        private static List<TypeWrapper> GetAllowedPatternTypes(Type type)
        {
            List<TypeWrapper> output = new();

            if (type.IsGenericType(typeof(Nullable<>)))
            {
                output.AddRange(GetAllowedPatternTypes(type.GenericTypeArguments[0]).Where(w => typeof(IPattern<>).MakeGenericType(type).IsAssignableFrom(w.Type)));
                output.Add(typeof(NullableNullPattern<>).MakeGenericType(type.GenericTypeArguments));
            }
            else if (type.IsGenericType(typeof(IList<>)))
            {
                output.Add(typeof(AnyPattern<>).MakeGenericType(type.GenericTypeArguments));
                output.Add(typeof(AllPattern<>).MakeGenericType(type.GenericTypeArguments));
            }
            else
            {
                if (Info.PatternParser.TryGetTypeDef(new TypeRef(type), out TypeDef typeDef))
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

        private void RefreshPatternTypes()
        {
            patternTypes.ReplaceRange(0, GetAllowedPatternTypes(valueType), patternTypes.Count);
        }
    }
}