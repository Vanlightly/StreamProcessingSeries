using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedStreamProcessing.ApacheKafka.Producer
{
    public class RawBusMessage
    {
        public int Counter { get; set; }
        public byte[] Data { get; set; }
        public DateTime ReadingTime { get; set; }
        public string MachineId { get; set; }
    }
}
