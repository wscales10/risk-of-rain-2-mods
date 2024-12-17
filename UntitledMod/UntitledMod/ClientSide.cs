using UnityEngine.Networking;

namespace UntitledMod
{
    public class ClientSide : ExecutionContext
    {
        public ClientSide(CustomLogger logger) : base(logger)
        {
        }

        protected override bool AllowExecution()
        {
            return !NetworkServer.active;
        }

        protected override string GetWarningMessage(int callerLineNumber, string callerFileName)
        {
            return $"Client-only code blocked on server at line ${callerLineNumber} of ${callerFileName}.";
        }
    }
}