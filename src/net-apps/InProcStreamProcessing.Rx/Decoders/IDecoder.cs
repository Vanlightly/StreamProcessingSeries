using InProcStreamProcessing.Shared.SensorConfiguration;
using InProcStreamProcessing.Rx.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace InProcStreamProcessing.Rx.Decoders
{
    public interface IDecoder
    {
        void LoadSensorConfigs();
        IEnumerable<DecodedMessage> Decode(RawBusMessage message);
    }
}
