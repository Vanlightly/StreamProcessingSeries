using System;
using System.Collections.Generic;
using System.Text;

namespace InProcStreamProcessing.TplDataFlow.Messages
{
    public class RawBusMessage
    {
        public int Counter { get; set; }
        public byte[] Data { get; set; }
        public DateTime ReadingTime { get; set; }
    }
}
