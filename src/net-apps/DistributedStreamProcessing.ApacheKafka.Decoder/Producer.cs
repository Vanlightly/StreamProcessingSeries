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
    public class Producer
    {
        public Task StartProducing(CancellationToken token, BlockingCollection<DecodedMessage> decodedMessages)
        {
            return Task.Run(async () => await ProduceAsync(token, decodedMessages));
        }

        private async Task ProduceAsync(CancellationToken token, BlockingCollection<DecodedMessage> decodedMessages)
        {
            var config = new Dictionary<string, object>
            {
                { "bootstrap.servers", "localhost:9092" }
            };

            using (var producer = new Producer<string, string>(config, new StringSerializer(Encoding.UTF8), new StringSerializer(Encoding.UTF8)))
            {
                while(!token.IsCancellationRequested && !decodedMessages.IsCompleted)
                {
                    DecodedMessage decodedMessage = null;
                    try
                    {
                        decodedMessage = decodedMessages.Take(); // blocks until message available or BlockingCollection completed
                    }
                    catch (InvalidOperationException) { }

                    if (decodedMessage != null)
                    {
                        var jsonText = JsonConvert.SerializeObject(decodedMessage);
                        var messageKey = $"{decodedMessage.MachineId}-{decodedMessage.Source}-{decodedMessage.Label}";
                        await producer.ProduceAsync("decoded", messageKey, jsonText);

                        Console.WriteLine($"Decoded message sent. Message Key: {messageKey}");
                    }
                }
            }
        }
    }
}
