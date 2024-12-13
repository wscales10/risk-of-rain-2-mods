using BepInEx.Logging;
using System.Runtime.CompilerServices;

namespace UntitledMod
{
    internal class CustomLogger
    {
        private readonly ManualLogSource logSource;

        public CustomLogger(ManualLogSource logSource)
        {
            this.logSource = logSource;
        }

        public void LogMethodCall([CallerFilePath]string callerFilePath = null, [CallerMemberName]string callerMemberName = null)
        {
            this.logSource.LogDebug($"{callerMemberName} in {callerFilePath}");
        }

        public void LogDebug(object data)
        {
            this.logSource.LogDebug(data);
        }
    }
}