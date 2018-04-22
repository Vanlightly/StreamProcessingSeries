
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using InProcStreamProcessing.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedStreamProcessing.ApacheKafka.Producer
{
    public class DataBusReader
    {
        private DataBusInterface _dataBus;
        private int _counter;

        public DataBusReader()
        {
            _dataBus = new DataBusInterface();
            _dataBus.Initialize();
        }

        public Task StartProducing(CancellationToken token, TimeSpan interval)
        {
            return Task.Factory.StartNew(() => Produce(token, interval), TaskCreationOptions.LongRunning);
        }

        private void Produce(CancellationToken token, TimeSpan interval)
        {
            var config = new Dictionary<string, object>
            {
                { "bootstrap.servers", "localhost:9092" }
            };

            var machineId = _dataBus.GetMachineId();

            using (var producer = new Producer<string, string>(config, new StringSerializer(Encoding.UTF8), new StringSerializer(Encoding.UTF8)))
            {
                long lastTicks = 0;
                while (!token.IsCancellationRequested)
                {
                    _counter++;
                    var reading = _dataBus.Read();

                    Console.WriteLine($"Read message {_counter} on thread {Thread.CurrentThread.ManagedThreadId}");

                    var message = new RawBusMessage();
                    message.Data = reading.Data;
                    message.ReadingTime = new DateTime(reading.Ticks);
                    message.Counter = _counter;
                    message.MachineId = machineId;

                    if (lastTicks < reading.Ticks)
                    {
                        var jsonText = JsonConvert.SerializeObject(message);
                        var sendResult = producer.ProduceAsync(topic: "raw", key: machineId, val: jsonText, blockIfQueueFull: true).Result;
                        if(sendResult.Error.HasError)
                            Console.WriteLine("Could not send: " + sendResult.Error.Reason);
                    }

                    lastTicks = reading.Ticks;

                    Thread.Sleep(interval);
                }
            }
        }
    }
}
