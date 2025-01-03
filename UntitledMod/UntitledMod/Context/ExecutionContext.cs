using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace UntitledMod.Context
{
    public abstract class ExecutionContext
    {
        private readonly ICustomLogger logger;

        protected ExecutionContext(ICustomLogger logger)
        {
            this.logger = logger;
        }

        public bool TryExecute(Action action, [CallerLineNumber] int callerLineNumber = -1, [CallerFilePath] string callerFilePath = null)
        {
            bool isExecutionAllowed = this.AllowExecution();

            if (isExecutionAllowed)
            {
                action();
            }
            else
            {
                this.logger.LogDebug(this.GetWarningMessage(callerLineNumber, Path.GetFileName(callerFilePath)));
            }

            return isExecutionAllowed;
        }

        protected abstract bool AllowExecution();

        protected abstract string GetWarningMessage(int callerLineNumber, string callerFileName);
    }
}