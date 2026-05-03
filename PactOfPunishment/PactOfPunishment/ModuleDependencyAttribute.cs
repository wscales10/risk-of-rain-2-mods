using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class ModuleDependencyAttribute : Attribute
{
    public ModuleDependencyAttribute(Type dependencyType) => DependencyType = dependencyType;

    public Type DependencyType { get; }
}