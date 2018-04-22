using System;
using System.Collections.Generic;
using System.Text;

namespace InProcStreamProcessing.Shared
{
    public class BusMessage
    {
        public byte[] Data { get; set; }
        public long Ticks { get; set; }
        public string XYZ { get; set; }
    }
}
