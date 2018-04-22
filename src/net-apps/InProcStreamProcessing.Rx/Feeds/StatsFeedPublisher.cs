using System;
using System.Collections.Generic;
using System.Linq;
using InProcStreamProcessing.Rx.Messages;
using System.Threading.Tasks;
using System.Threading;

namespace InProcStreamProcessing.Rx.Feeds
{
    public class StatsFeedPublisher : IStatsFeedPublisher
    {
        private int _inProgress = 0;

        public async Task PublishAsync(IList<DecodedMessage> messages, TimeSpan period)
        {
            Interlocked.Increment(ref _inProgress);
            if (ShowMessages.PrintStatsFeed)
                Console.WriteLine($"                        Publish stats of {(int)period.TotalSeconds} second batch of {messages.Count} messages with Counters from {messages.First().Counter} to {messages.Last().Counter}. In Progress: {_inProgress} on thread {Thread.CurrentThread.ManagedThreadId}");

            // simulate the work here
            await Task.Delay(1);
            Interlocked.Decrement(ref _inProgress);
        }
    }
}
