using System;
using System.Collections.Generic;
using System.Text;

namespace InProcStreamProcessing.Shared.SensorConfiguration
{
    public class SensorConfig
    {
        public string ComponentCode { get; set; }
        public string SensorCode { get; set; }
        public string Unit { get; set; }
        public int ByteIndex { get; set; }
        public int ByteCount { get; set; }
        public double Precision { get; set; }
        public bool Signed { get; set; }
        public double UpperRange { get; set; }
        public double LowerRange { get; set; }
    }
}
