using System;
using System.Collections.Generic;
using System.Text;
using InProcStreamProcessing.TplDataFlow.Messages;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace InProcStreamProcessing.TplDataFlow.Feeds
{
    public class StatsFeedPublisher : IStatsFeedPublisher
    {
        private int _inProgress = 0;

        public async Task PublishAsync(IList<DecodedMessage> messages, TimeSpan window)
        {
            Interlocked.Increment(ref _inProgress);

            if (ShowMessages.PrintStatsFeed)
                Console.WriteLine($"                        Publish {(int)window.TotalSeconds} seconds of stats of batch of {messages.Count} with Counters from {messages.First().Counter} to {messages.Last().Counter}. In Progress: {_inProgress} on thread {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(1);

            Interlocked.Decrement(ref _inProgress);
        }
    }
}
