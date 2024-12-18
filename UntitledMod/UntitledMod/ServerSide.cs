using UnityEngine.Networking;

namespace UntitledMod
{
    public class ServerSide : ExecutionContext
    {
        public ServerSide(ICustomLogger logger) : base(logger)
        {
        }

        protected override bool AllowExecution()
        {
            return NetworkServer.active;
        }

        protected override string GetWarningMessage(int callerLineNumber, string callerFileName)
        {
            return $"Server-only code blocked on client at line ${callerLineNumber} of ${callerFileName}.";
        }
    }
}