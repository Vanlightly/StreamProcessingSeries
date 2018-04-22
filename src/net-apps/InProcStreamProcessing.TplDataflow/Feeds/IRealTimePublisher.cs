using InProcStreamProcessing.TplDataFlow.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InProcStreamProcessing.TplDataFlow.Feeds
{
    public interface IRealTimePublisher
    {
        Task PublishAsync(DecodedMessage message);
    }
}
