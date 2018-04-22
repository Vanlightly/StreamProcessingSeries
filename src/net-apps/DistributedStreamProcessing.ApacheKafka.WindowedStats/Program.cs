using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DistributedStreamProcessing.ApacheKafka.WindowedStats
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Statistics";

            var producerCts = new CancellationTokenSource();

            var oneSecondGroupMessages = new BlockingCollection<DecodedMessage>();
            var oneSecondProducer = new Producer();
            oneSecondProducer.StartProducing(producerCts.Token, TimeSpan.FromSeconds(1), "one-second-stats", oneSecondGroupMessages);

            var thirtySecondGroupMessages = new BlockingCollection<DecodedMessage>();
            var thirtySecondProducer = new Producer();
            thirtySecondProducer.StartProducing(producerCts.Token, TimeSpan.FromSeconds(30), "thirty-second-stats", thirtySecondGroupMessages);

            var consumerCts = new CancellationTokenSource();
            var oneSecondConsumer = new Consumer();
            oneSecondConsumer.StartConsuming(consumerCts.Token, "one-second-group", oneSecondGroupMessages);

            var thirtySecondConsumer = new Consumer();
            thirtySecondConsumer.StartConsuming(consumerCts.Token, "thirty-second-group", thirtySecondGroupMessages);

            Console.WriteLine("Press any key to shutdown");
            Console.ReadKey();

            consumerCts.Cancel();

            // perform a shutdown, allowing for 30 seconds to process current items
            // this is just an example, it could end with message loss
            var sw = new Stopwatch();
            sw.Start();
            while (oneSecondGroupMessages.Any())
            {
                if (sw.ElapsedMilliseconds > 60000)
                    producerCts.Cancel(); // force close (could lose messages)
                else
                    Thread.Sleep(100);
            }
        }
    }
}
