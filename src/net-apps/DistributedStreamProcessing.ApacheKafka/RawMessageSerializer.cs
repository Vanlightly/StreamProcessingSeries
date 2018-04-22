using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedStreamProcessing.ApacheKafka.Producer
{
    public class RawMessageSerializer : ISerializer<RawBusMessage>
    {
        public IEnumerable<KeyValuePair<string, object>> Configure(IEnumerable<KeyValuePair<string, object>> config, bool isKey)
        {
            return config;
        }

        public void Dispose()
        {
            
        }

        public byte[] Serialize(string topic, RawBusMessage data)
        {
            int len = data.Data.Length + 12;
            var bytes = new Byte[len];
            Array.Copy(BitConverter.GetBytes(data.Counter), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(data.ReadingTime.Ticks), 0, bytes, 4, 8);
            Array.Copy(data.Data, 0, bytes, 12, len);

            return bytes;
        }
    }
}
