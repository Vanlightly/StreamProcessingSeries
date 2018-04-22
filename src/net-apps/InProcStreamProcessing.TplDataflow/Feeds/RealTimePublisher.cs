using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using InProcStreamProcessing.TplDataFlow.Messages;
using System.Threading;

namespace InProcStreamProcessing.TplDataFlow.Feeds
{
    public class RealTimePublisher : IRealTimePublisher
    {
        public async Task PublishAsync(DecodedMessage message)
        {
            // send over a network socket
            if (ShowMessages.PrintRealTimeFeed)
                Console.WriteLine($"            Publish in real-time message Sensor: { message.Label} { message.Value} { message.Unit} on thread {Thread.CurrentThread.ManagedThreadId}");
            await Task.Yield();
        }
    }
}
