using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using InProcStreamProcessing.Shared.SensorConfiguration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace DistributedStreamProcessing.ApacheKafka.Decoder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Decoder";

            var producerCts = new CancellationTokenSource();
            var decodedMessages = new BlockingCollection<DecodedMessage>(boundedCapacity: 1000);
            var producer = new Producer();
            producer.StartProducing(producerCts.Token, decodedMessages);

            var consumerCts = new CancellationTokenSource();
            var consumer = GetConsumer();
            consumer.StartConsuming(consumerCts.Token, decodedMessages);

            Console.WriteLine("Press any key to shutdown");
            Console.ReadKey();

            consumerCts.Cancel();

            // perform a shutdown, allowing for 30 seconds to process current items
            // this is just an example, it could end with message loss
            var sw = new Stopwatch();
            sw.Start();
            while (decodedMessages.Any())
            {
                if (sw.ElapsedMilliseconds > 60000)
                    producerCts.Cancel(); // force close (could lose messages)
                else
                    Thread.Sleep(100);
            }
        }

        private static Consumer GetConsumer()
        {
            // use your favourite IoC here
            return new Consumer(new Decoder(new ConfigurationLoader()));
        }
    }
}
