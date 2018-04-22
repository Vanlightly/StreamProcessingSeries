using InProcStreamProcessing.Rx.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InProcStreamProcessing.Rx.Feeds
{
    public interface IRealTimePublisher
    {
        Task PublishAsync(DecodedMessage message);
    }
}
