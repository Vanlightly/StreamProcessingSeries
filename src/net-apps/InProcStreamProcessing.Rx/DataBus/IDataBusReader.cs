using InProcStreamProcessing.Rx.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InProcStreamProcessing.Rx.DataBus
{
    public interface IDataBusReader
    {
        IObservable<RawBusMessage> StartConsuming(CancellationToken token, TimeSpan interval);
    }
}
