using System.Runtime.CompilerServices;

namespace UntitledMod
{
    public interface ICustomLogger
    {
        void LogDebug(object data);

        void LogError(object data);

        void LogVerbose(object data);

        void LogMethodCall([CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerMemberName = null);
    }
}