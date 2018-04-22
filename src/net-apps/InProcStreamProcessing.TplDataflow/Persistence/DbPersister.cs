using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InProcStreamProcessing.TplDataFlow.Messages;
using System.Threading;

namespace InProcStreamProcessing.TplDataFlow.Persistence
{
    public class DbPersister : IDbPersister
    {
        private int _inProgress = 0;

        public async Task PersistAsync(IList<DecodedMessage> messages)
        {
            Interlocked.Increment(ref _inProgress);
            if (ShowMessages.PrintDbPersist)
                Console.WriteLine($"                        Persisted batch of {messages.Count} with Counters from {messages.First().Counter} to {messages.Last().Counter}. In Progress: {_inProgress} on thread {Thread.CurrentThread.ManagedThreadId}");

            await Task.Delay(10000);
            Interlocked.Decrement(ref _inProgress);
        }
    }
}
