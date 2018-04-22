using InProcStreamProcessing.Shared;
using InProcStreamProcessing.TplDataFlow.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace InProcStreamProcessing.TplDataFlow.DataBus
{
    public class DataBusReader : IDataBusReader
    {
        private DataBusInterface _dataBus;
        private int _counter;

        public DataBusReader()
        {
            _dataBus = new DataBusInterface();
            _dataBus.Initialize();
        }

        public Task StartConsuming(ITargetBlock<RawBusMessage> target, CancellationToken token, TimeSpan interval, FlowControlMode flowControlMode)
        {
            if(flowControlMode == FlowControlMode.LoadShed)
                return Task.Factory.StartNew(() => ConsumeWithDiscard(target, token, interval), TaskCreationOptions.LongRunning);
            else
                return Task.Factory.StartNew(() => ConsumeWithBackPressure(target, token, interval), TaskCreationOptions.LongRunning);
        }

        private void ConsumeWithDiscard(ITargetBlock<RawBusMessage> target, CancellationToken token, TimeSpan interval)
        {
            long lastTicks = 0;
            while (!token.IsCancellationRequested)
            {
                _counter++;
                var reading = _dataBus.Read();

                if(ShowMessages.PrintReader)
                    Console.WriteLine($"Read message {_counter} on thread {Thread.CurrentThread.ManagedThreadId}");

                var message = new RawBusMessage();
                message.Data = reading.Data;
                message.ReadingTime = new DateTime(reading.Ticks);
                message.Counter = _counter;

                if (lastTicks < reading.Ticks)
                {
                    var posted = target.Post(message);
                    if (!posted && ShowMessages.PrintFullBuffers)
                        Console.WriteLine("Buffer full. Could not post");
                }

                lastTicks = reading.Ticks;

                Thread.Sleep(interval);
            }
        }

        private void ConsumeWithBackPressure(ITargetBlock<RawBusMessage> target, CancellationToken token, TimeSpan interval)
        {
            long lastTicks = 0;
            while (!token.IsCancellationRequested)
            {
                _counter++;
                var reading = _dataBus.Read();

                if (ShowMessages.PrintReader)
                    Console.WriteLine($"Read message {_counter} on thread {Thread.CurrentThread.ManagedThreadId}");

                var message = new RawBusMessage();
                message.Data = reading.Data;
                message.ReadingTime = new DateTime(reading.Ticks);
                message.Counter = _counter;

                if (lastTicks < reading.Ticks)
                {
                    while (!target.Post(message))
                        Thread.Sleep((int)interval.TotalMilliseconds);
                }

                lastTicks = reading.Ticks;

                Thread.Sleep(interval);
            }
        }
    }
}
