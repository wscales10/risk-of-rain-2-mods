using BepInEx.Logging;
using System.Runtime.CompilerServices;

namespace UntitledMod
{
    internal class CustomLogger : ICustomLogger
    {
        private readonly ManualLogSource logSource;

        public CustomLogger(ManualLogSource logSource)
        {
            this.logSource = logSource;
        }

        public void LogMethodCall([CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null)
        {
            this.logSource.LogDebug($"{callerMemberName} in {callerFilePath}");
        }

        public void LogDebug(object data)
        {
            this.logSource.LogInfo(data);
        }

        public void LogError(object data)
        {
            this.logSource.LogError(data);
        }

        public void LogVerbose(object data)
        {
            this.logSource.LogDebug(data);
        }
    }
}