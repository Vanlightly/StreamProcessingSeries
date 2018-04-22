using InProcStreamProcessing.Shared.SensorConfiguration;
using InProcStreamProcessing.TplDataFlow.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace InProcStreamProcessing.TplDataFlow.Decoders
{
    public interface IDecoder
    {
        void LoadSensorConfigs();
        IEnumerable<DecodedMessage> Decode(RawBusMessage message);
    }
}
