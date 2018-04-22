using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedStreamProcessing.ApacheKafka.Decoder
{
    public class RawBusMessage
    {
        public int Counter { get; set; }
        public byte[] Data { get; set; }
        public DateTime ReadingTime { get; set; }
        public string MachineId { get; set; }
    }
}
