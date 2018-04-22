using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedStreamProcessing.ApacheKafka.Decoder
{
    public interface IDecoder
    {
        void LoadSensorConfigs();
        IEnumerable<DecodedMessage> Decode(RawBusMessage message);
    }
}
