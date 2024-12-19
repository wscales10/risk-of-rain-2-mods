using R2API.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace UntitledMod.Tests
{
    internal class TestsEntryPoint
    {
        private readonly ICustomLogger logger;

        public TestsEntryPoint(ICustomLogger logger)
        {
            this.logger = logger;
        }

        public void RunAllTests()
        {
            int passes = 0, tests = 0;
            foreach (var testClass in Assembly.GetAssembly(this.GetType()).GetTypes()
                .Where(t => t.IsSubclassOf(typeof(TestClass))))
            {
                foreach (var testMethod in testClass.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    var instance = testClass.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
                    instance.SetPropertyValue(nameof(TestClass.Logger), this.logger);

                    tests++;

                    try
                    {
                        testMethod.Invoke(instance, Array.Empty<object>());
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex);
                        this.logger.LogDebug($"Test '{testClass.Name}_{testMethod.Name}' FAILED");
                        continue;
                    }

                    passes++;
                    this.logger.LogDebug($"Test '{testClass.Name}_{testMethod.Name}' PASSED");
                }
            }

            this.logger.LogDebug($"{passes} / {tests} tests passed.");
        }
    }
}