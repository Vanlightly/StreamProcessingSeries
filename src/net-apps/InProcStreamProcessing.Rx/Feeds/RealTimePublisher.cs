using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using InProcStreamProcessing.Rx.Messages;
using System.Threading;

namespace InProcStreamProcessing.Rx.Feeds
{
    public class RealTimePublisher : IRealTimePublisher
    {
        public async Task PublishAsync(DecodedMessage message)
        {
            // send over a network socket
            if(ShowMessages.PrintRealTimeFeed)
                Console.WriteLine($"            Publish in real-time message {message.Counter} Sensor: { message.Label} { message.Value} { message.Unit} on thread {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(1);
        }
    }
}
