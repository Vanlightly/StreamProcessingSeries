using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedStreamProcessing.ApacheKafka.WindowedStats
{
    public class StatsWindowMessage
    {
        public string MachineId { get; set; }
        public string Source { get; set; }
        public string Label { get; set; }
        public DateTime PeriodStart { get; set; }
        public TimeSpan Window { get; set; }
        public string Unit { get; set; }
        public double Count { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double AvgValue { get; set; }
    }
}
