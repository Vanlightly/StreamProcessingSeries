using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DistributedStreamProcessing.ApacheKafka.Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Topology Producer";

            var source = new CancellationTokenSource();
            var producer = new DataBusReader();
            var producerTask = producer.StartProducing(source.Token, TimeSpan.FromMilliseconds(100));

            Console.WriteLine("Press any key to shutdown");
            Console.ReadKey();

            source.Cancel();
            producerTask.Wait();
        }
    }
}
