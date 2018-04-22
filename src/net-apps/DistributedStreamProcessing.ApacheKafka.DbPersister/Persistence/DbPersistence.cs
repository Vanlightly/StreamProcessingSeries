using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DistributedStreamProcessing.ApacheKafka.DbPersister.Persistence
{
    public class DbPersistence : IDbPersister
    {
        private int _inProgress = 0;

        public void StartPersisting(CancellationToken token, TimeSpan interval, BlockingCollection<DecodedMessage> decodedMessages)
        {
            Task.Run(async () => await StartPeriodicPersistenceAsync(token, interval, decodedMessages));
        }

        private async Task StartPeriodicPersistenceAsync(CancellationToken token, TimeSpan interval, BlockingCollection<DecodedMessage> decodedMessages)
        {
            while(!token.IsCancellationRequested && !decodedMessages.IsAddingCompleted)
            {
                var batch = new List<DecodedMessage>();

                DecodedMessage message = null;
                while (decodedMessages.TryTake(out message))
                    batch.Add(message);

                if(batch.Any())
                    await PersistAsync(batch);

                await Task.Delay(interval);
            }
        }

        private async Task PersistAsync(IList<DecodedMessage> messages)
        {
            Interlocked.Increment(ref _inProgress);
            Console.WriteLine($"Persisted batch of {messages.Count} with Counters from {messages.First().Counter} to {messages.Last().Counter}. In Progress: {_inProgress} on thread {Thread.CurrentThread.ManagedThreadId}");

            await Task.Delay(100);
            Interlocked.Decrement(ref _inProgress);
        }
    }
}
