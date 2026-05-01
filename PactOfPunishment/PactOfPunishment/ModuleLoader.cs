using System;
using System.Collections.Generic;
using System.Linq;

namespace PactOfPunishment
{
    public class ModuleLoader
    {
        private readonly Dictionary<Type, Module> moduleTypeToInstance;

        private readonly List<Module> sortedModules;

        private readonly HashSet<Type> visited;

        private readonly HashSet<Type> stack;

        public ModuleLoader(IEnumerable<Module> modules)
        {
            this.moduleTypeToInstance = modules.ToDictionary(m => m.GetType());
            this.sortedModules = new List<Module>();
            this.visited = new HashSet<Type>();
            this.stack = new HashSet<Type>();

            foreach (var type in this.moduleTypeToInstance.Keys)
            {
                this.Visit(type);
            }
        }

        public static IEnumerable<Type> GetModuleTypeDependencies(Type moduleType)
        {
            return moduleType.GetCustomAttributes(typeof(ModuleDependencyAttribute), false)
                .Cast<ModuleDependencyAttribute>()
                .Select(attr => attr.DependencyType);
        }

        public static List<Module> SortModulesByDependencies(IEnumerable<Module> modules)
        {
            return new ModuleLoader(modules).sortedModules;
        }

        private void Visit(Type moduleType)
        {
            if (!this.visited.Add(moduleType))
            {
                return;
            }

            if (!this.stack.Add(moduleType))
            {
                throw new InvalidOperationException($"Circular module dependency detected: {string.Join(" > ", this.stack.Select(x => x.FullName))}");
            }

            var dependencies = GetModuleTypeDependencies(moduleType);

            foreach (var dependency in dependencies.Where(dependency => this.moduleTypeToInstance.ContainsKey(dependency)))
            {
                this.Visit(dependency);
            }

            this.stack.Remove(moduleType);

            if (this.moduleTypeToInstance.TryGetValue(moduleType, out var module) && !this.sortedModules.Contains(module))
            {
                this.sortedModules.Add(module);
            }
        }
    }
}