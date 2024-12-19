using UnityEngine.Networking;

namespace UntitledMod
{
    public class ServerSide : ExecutionContext
    {
        private readonly IRoR2Context gameContext;

        public ServerSide(ICustomLogger logger, IRoR2Context gameContext) : base(logger)
        {
            this.gameContext = gameContext;
        }

        protected override bool AllowExecution()
        {
            return this.gameContext.IsNetworkServerActive;
        }

        protected override string GetWarningMessage(int callerLineNumber, string callerFileName)
        {
            return $"Server-only code blocked on client at line ${callerLineNumber} of ${callerFileName}.";
        }
    }
}