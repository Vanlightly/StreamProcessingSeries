using DistributedStreamProcessing.ApacheKafka.DbPersister.Persistence;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DistributedStreamProcessing.ApacheKafka.DbPersister
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "DB Persister";
            var decodedMessages = new BlockingCollection<DecodedMessage>();

            var consumerCts = new CancellationTokenSource();
            var consumer = new Consumer();
            consumer.StartConsuming(consumerCts.Token, decodedMessages);

            var dbPersisterCts = new CancellationTokenSource();
            var dbPersister = new DbPersistence();
            dbPersister.StartPersisting(dbPersisterCts.Token, TimeSpan.FromSeconds(10), decodedMessages);

            Console.WriteLine("Press any key to shutdown");
            Console.ReadKey();
            consumerCts.Cancel();

            // perform a shutdown, allowing for 60 seconds to process current items
            // this is just an example, it could end with message loss
            var sw = new Stopwatch();
            sw.Start();
            while (decodedMessages.Any())
            {
                if (sw.ElapsedMilliseconds > 60000)
                    dbPersisterCts.Cancel(); // force close (could lose messages)
                else
                    Thread.Sleep(100);
            }
        }
    }
}
