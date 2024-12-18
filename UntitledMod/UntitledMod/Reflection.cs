using System;

namespace UntitledMod
{
    public static class Reflection
    {
        public static void RaiseStaticEvent(this Type type, string name, params object[] parameters)
        {
            type.GetEvent(name).RaiseMethod?.Invoke(null, parameters);
        }

        public static void RaiseInstanceEvent<T>(this T instance, string name, params object[] parameters)
        {
            typeof(T).GetEvent(name).RaiseMethod?.Invoke(instance, parameters);
        }
    }
}
