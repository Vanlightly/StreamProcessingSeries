using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using InProcStreamProcessing.Rx.Messages;
using System.Linq;
using System.Threading;

namespace InProcStreamProcessing.Rx.Persistence
{
    public class DbPersister : IDbPersister
    {
        private int _inProgress = 0;

        public async Task PersistAsync(IList<DecodedMessage> messages)
        {
            Interlocked.Increment(ref _inProgress);
            if (ShowMessages.PrintDbPersist)
                Console.WriteLine($"                        Persisted batch of {messages.Count} with Counters from {messages.First().Counter} to {messages.Last().Counter}. In Progress: {_inProgress} on thread {Thread.CurrentThread.ManagedThreadId}");

            await Task.Delay(100);
            Interlocked.Decrement(ref _inProgress);
        }
    }
}
