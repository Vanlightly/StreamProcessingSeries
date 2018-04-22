using System;
using System.Collections.Generic;
using System.Text;

namespace InProcStreamProcessing.Rx.Messages
{
    public class DecodedMessage
    {
        public int Counter { get; set; }
        public string Unit { get; set; }
        public double Value { get; set; }
        public string Source { get; set; }
        public string Label { get; set; }
        public DateTime ReadingTime { get; set; }
    }
}
