using InProcStreamProcessing.Rx.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InProcStreamProcessing.Rx.Persistence
{
    public interface IMessageFileWriter
    {
        void Open();
        Task WriteAsync(RawBusMessage message);
        void Close();
    }
}
