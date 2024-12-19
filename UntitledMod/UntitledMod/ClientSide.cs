namespace UntitledMod
{
    public class ClientSide : ExecutionContext
    {
        private readonly IRoR2Context gameContext;

        public ClientSide(ICustomLogger logger, IRoR2Context gameContext) : base(logger)
        {
            this.gameContext = gameContext;
        }

        protected override bool AllowExecution()
        {
            return !this.gameContext.IsNetworkServerActive;
        }

        protected override string GetWarningMessage(int callerLineNumber, string callerFileName)
        {
            return $"Client-only code blocked on server at line ${callerLineNumber} of ${callerFileName}.";
        }
    }
}