using System;
using System.Collections.Generic;
using System.Text;
using InProcStreamProcessing.Rx.Messages;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace InProcStreamProcessing.Rx.Persistence
{
    public class MessageFileWriter : IMessageFileWriter
    {
        public void Open()
        {
            // open some kind of file io
        }

        public void Close()
        {
            // safely shutdown the file io
        }

        public async Task WriteAsync(RawBusMessage message)
        {
            if (ShowMessages.PrintFileWriter)
                Console.WriteLine($"Write to file message: {message.Counter} on thread {Thread.CurrentThread.ManagedThreadId}");
            await Task.Yield();
        }
    }
}
