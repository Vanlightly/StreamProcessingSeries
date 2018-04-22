using InProcStreamProcessing.TplDataFlow.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace InProcStreamProcessing.TplDataFlow.DataBus
{
    public interface IDataBusReader
    {
        Task StartConsuming(ITargetBlock<RawBusMessage> target, CancellationToken token, TimeSpan interval, FlowControlMode flowControlMode);
    }
}
