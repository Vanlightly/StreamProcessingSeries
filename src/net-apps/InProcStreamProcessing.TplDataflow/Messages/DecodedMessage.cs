using System;
using System.Collections.Generic;
using System.Text;

namespace InProcStreamProcessing.TplDataFlow.Messages
{
    public class DecodedMessage
    {
        public string Unit { get; set; }
        public double Value { get; set; }
        public string Source { get; set; }
        public string Label { get; set; }
        public DateTime ReadingTime { get; set; }
        public double Counter { get; set; }
    }
}
