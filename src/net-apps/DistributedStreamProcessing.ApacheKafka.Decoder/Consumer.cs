using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedStreamProcessing.ApacheKafka.Decoder
{
    public class Consumer
    {
        private readonly IDecoder _decoder;

        public Consumer(IDecoder decoder)
        {
            _decoder = decoder;
        }

        public Task StartConsuming(CancellationToken token, BlockingCollection<DecodedMessage> decodedMessages)
        {
            _decoder.LoadSensorConfigs();
            return Task.Run(() => Consume(token, decodedMessages));
        }

        private void Consume(CancellationToken token, BlockingCollection<DecodedMessage> decodedMessages)
        {
            var conf = new Dictionary<string, object>
            {
                  { "group.id", "decoder-consumer-group" },
                  { "bootstrap.servers", "localhost:9092" },
                  { "auto.commit.interval.ms", 5000 },
                  { "auto.offset.reset", "earliest" }
            };

            using (var consumer = new Consumer<string, string>(conf, new StringDeserializer(Encoding.UTF8), new StringDeserializer(Encoding.UTF8)))
            {
                consumer.OnMessage += (_, msg) => Decode(msg.Offset.Value, msg.Key, msg.Value, decodedMessages);

                consumer.OnError += (_, error)
                  => Console.WriteLine($"Error: {error}");

                consumer.OnConsumeError += (_, msg)
                  => Console.WriteLine($"Consume error ({msg.TopicPartitionOffset}): {msg.Error}");

                consumer.Subscribe("raw");

                while (!token.IsCancellationRequested)
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));
                }
            }

            decodedMessages.CompleteAdding(); // notifies consumers that no more messages will come
        }

        private void Decode(long offset, string key, string jsonMessage, BlockingCollection<DecodedMessage> decodedMessages)
        {
            Console.WriteLine($"Raw message consumed with offset: {offset}, key: {key}");
            var rawBusMessage = JsonConvert.DeserializeObject<RawBusMessage>(jsonMessage);
            var decoded = _decoder.Decode(rawBusMessage);

            foreach (var decodedMessage in decoded)
                decodedMessages.Add(decodedMessage); // blocks once bounded capacity reached
        }
    }
}
