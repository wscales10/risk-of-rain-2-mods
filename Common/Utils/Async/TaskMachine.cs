using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Utils.Async
{
    public abstract class TaskMachine
    {
        private readonly Channel<(CancellableTask, CancellationToken)> tasks;

        private Task lifecycle;

        private (CancellableTask, CancellationToken) lastRead;

        protected TaskMachine(Channel<(CancellableTask, CancellationToken)> tasks)
        {
            this.tasks = tasks;
        }

        public Task Lifecycle
        {
            get => lifecycle;

            private set
            {
                if (lifecycle is null)
                {
                    lifecycle = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        protected abstract CancellationToken MasterCancellationToken { get; }

        public void Close()
        {
            tasks.Writer.Complete();
        }

        public ConditionalValue<CancellableTask> TryIngest(Func<CancellationToken, Task> func, CancellationToken cancellationToken = default) => TryIngest(new CancellableTask(func), cancellationToken);

        public ConditionalValue<CancellableTask> TryIngest(CancellableTask task, CancellationToken cancellationToken = default)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (tasks.Writer.TryWrite((task, cancellationToken)))
            {
                return new ConditionalValue<CancellableTask>(task);
            }
            else
            {
                return new ConditionalValue<CancellableTask>();
            }
        }

        protected void StartListening() => Lifecycle = ExecuteAsync();

        private async Task ExecuteAsync()
        {
            this.Log("TaskMachine lifecycle start");
            while (await tasks.Reader.WaitToReadAsync(MasterCancellationToken))
            {
                this.Log("TaskMachine task read");
                var (cancellableTask, cancellationToken) = lastRead = await tasks.Reader.ReadAsync(MasterCancellationToken);
                MaybeCancel();
                var combinedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(MasterCancellationToken, cancellationToken).Token;
                if (!combinedCancellationToken.IsCancellationRequested)
                {
                    await cancellableTask.RunAsync(combinedCancellationToken);
                }
                MaybeCancel();
                this.Log("TaskMachine task finished");
            }
            this.Log("TaskMachine lifecycle end");

            void MaybeCancel()
            {
                MasterCancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    public abstract class UnboundedTaskMachine : TaskMachine
    {
        protected UnboundedTaskMachine() : base(Channel.CreateUnbounded<(CancellableTask, CancellationToken)>())
        {
        }
    }

    public class JuniorTaskMachine : UnboundedTaskMachine
    {
        public JuniorTaskMachine(CancellationToken cancellationToken)
        {
            MasterCancellationToken = cancellationToken;
            StartListening();
        }

        protected override CancellationToken MasterCancellationToken { get; }
    }

    public class SeniorTaskMachine : UnboundedTaskMachine
    {
        public SeniorTaskMachine() => StartListening();

        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        protected override CancellationToken MasterCancellationToken => CancellationTokenSource.Token;
    }
}