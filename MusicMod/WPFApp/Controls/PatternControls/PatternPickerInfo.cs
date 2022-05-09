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

namespace WPFApp.Controls.PatternControls
{
    public class PatternPickerInfo : IPickerInfo
    {
        public PatternPickerInfo(Type valueType, NavigationContext navigationContext)
        {
            ValueType = valueType;
            NavigationContext = navigationContext;
        }

        public string DisplayMemberPath => nameof(TypeWrapper.DisplayName);

        public string SelectedValuePath => nameof(TypeWrapper.Type);

        public Type ValueType { get; }

        public NavigationContext NavigationContext { get; }

        public IEnumerable GetItems()
        {
            return GetAllowedPatternTypes(ValueType);
        }

        public IReadableControlWrapper CreateWrapper(object selectedInfo)
        {
            return PatternWrapper.Create((Type)selectedInfo, NavigationContext);
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
    }
}