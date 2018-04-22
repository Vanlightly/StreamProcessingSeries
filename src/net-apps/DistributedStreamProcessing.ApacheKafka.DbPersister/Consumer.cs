using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using DistributedStreamProcessing.ApacheKafka.DbPersister.Persistence;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedStreamProcessing.ApacheKafka.DbPersister
{
    public class Consumer
    {
        public Task StartConsuming(CancellationToken token, BlockingCollection<DecodedMessage> decodedMessages)
        {
            return Task.Run(() => Consume(token, decodedMessages));
        }

        private void Consume(CancellationToken token, BlockingCollection<DecodedMessage> decodedMessages)
        {
            var conf = new Dictionary<string, object>
            {
                  { "group.id", "db-persister-consumer-group" },
                  { "bootstrap.servers", "localhost:9092" },
                  { "auto.commit.interval.ms", 5000 },
                  { "auto.offset.reset", "earliest" }
            };

            using (var consumer = new Consumer<string, string>(conf, new StringDeserializer(Encoding.UTF8), new StringDeserializer(Encoding.UTF8)))
            {
                consumer.OnMessage += (_, msg) => Persist(msg.Offset.Value, msg.Key, msg.Value, decodedMessages);

                consumer.OnError += (_, error)
                  => Console.WriteLine($"Error: {error}");

                consumer.OnConsumeError += (_, msg)
                  => Console.WriteLine($"Consume error ({msg.TopicPartitionOffset}): {msg.Error}");

                consumer.Subscribe("decoded");

                while (!token.IsCancellationRequested)
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));
                }
            }
        }

        private void Persist(long offset, string key, string jsonMessage, BlockingCollection<DecodedMessage> decodedMessages)
        {
            var decodedMessage = JsonConvert.DeserializeObject<DecodedMessage>(jsonMessage);
            Console.WriteLine($"Decoded message added to buffer with offset: {offset}, key: {key}");
            decodedMessages.Add(decodedMessage); // blocks once bounded capacity reached
        }
    }
}
