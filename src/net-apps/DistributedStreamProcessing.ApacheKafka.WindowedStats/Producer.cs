using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedStreamProcessing.ApacheKafka.WindowedStats
{
    public class Producer
    {
        public void StartProducing(CancellationToken token, TimeSpan windowSize, string topic, BlockingCollection<DecodedMessage> decodedMessages)
        {
            Task.Run(async () => await ProduceAsync(token, windowSize, topic, decodedMessages));
        }

        private async Task ProduceAsync(CancellationToken token, TimeSpan windowSize, string topic, BlockingCollection<DecodedMessage> decodedMessages)
        {
            var config = new Dictionary<string, object>
            {
                { "bootstrap.servers", "localhost:9092" }
            };

            using (var producer = new Producer<string, string>(config, new StringSerializer(Encoding.UTF8), new StringSerializer(Encoding.UTF8)))
            {
                var sw = new Stopwatch();
                sw.Start();

                while (!token.IsCancellationRequested && !decodedMessages.IsCompleted)
                {
                    var batch = new List<DecodedMessage>();

                    DecodedMessage message = null;
                    while (sw.Elapsed < windowSize)
                    {
                        if(decodedMessages.TryTake(out message, TimeSpan.FromMilliseconds(100)))
                            batch.Add(message);
                    }

                    sw.Restart();
                    if (batch.Any())
                    {
                        var grouped = batch.GroupBy(x => new { x.MachineId, x.Source, x.Label });
                        foreach (var group in grouped)
                        {
                            var statsMessage = new StatsWindowMessage();
                            statsMessage.MachineId = group.Key.MachineId;
                            statsMessage.Source = group.Key.Source;
                            statsMessage.Label = group.Key.Label;
                            statsMessage.Unit = group.First().Unit;
                            statsMessage.PeriodStart = group.Min(x => x.ReadingTime);
                            statsMessage.MinValue = group.Min(x => x.Value);
                            statsMessage.MaxValue = group.Max(x => x.Value);
                            statsMessage.AvgValue = group.Average(x => x.Value);
                            statsMessage.Count = group.Count();

                            var jsonText = JsonConvert.SerializeObject(statsMessage);
                            var messageKey = $"{statsMessage.MachineId}-{statsMessage.Source}-{statsMessage.Label}";
                            await producer.ProduceAsync(topic, messageKey, jsonText);
                            Console.WriteLine($"Published to topic {topic} with key: {messageKey}");
                        }
                    }
                }
            }
        }
    }
}
