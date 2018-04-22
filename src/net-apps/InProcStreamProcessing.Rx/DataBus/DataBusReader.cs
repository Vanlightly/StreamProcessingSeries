using InProcStreamProcessing.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using InProcStreamProcessing.Rx.Messages;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using System.Collections.Concurrent;

namespace InProcStreamProcessing.Rx.DataBus
{
    public class DataBusReader : IDataBusReader
    {
        private DataBusInterface _dataBus;
        private int _counter = 0;
        
        public DataBusReader()
        {
            _dataBus = new DataBusInterface();
            _dataBus.Initialize();
        }

        public IObservable<RawBusMessage> StartConsuming(CancellationToken token, TimeSpan interval)
        {
            //-----------------------------------
            //--------- Observable.Generate
            var scheduler = new NewThreadScheduler(ts => new Thread(ts) { Name = "DataBusPoller" });
            var source = Observable.Generate(Read(),
                x => !token.IsCancellationRequested,
                x => Read(),
                x => x,
                x => interval,
                scheduler);

            return source;

            //-----------------------------------
            //--------- Observable.Create
            //IObservable<RawBusMessage> source = Observable.Create(async (IObserver<RawBusMessage> observer) =>
            //{
            //    try
            //    {
            //        while (!token.IsCancellationRequested)
            //        {
            //            observer.OnNext(Read());
            //            await Task.Delay(interval);
            //        }

            //        observer.OnCompleted();
            //    }
            //    catch (Exception ex)
            //    {
            //        observer.OnError(ex);
            //    }

            //    return Disposable.Empty;
            //});

            //return source;

            //-----------------------------------
            //--------- Observable.Timer
            //var scheduler = new EventLoopScheduler(ts => new Thread(ts) { Name = "DataBusPoller" });

            //var query = Observable.Timer(interval, scheduler)
            //                .Select(_ => Read())
            //                .Repeat();

            //return query;
        }

        // I didn't use this, but you can also produce to a BlockingCollection and turn that into an observable
        public BlockingCollection<RawBusMessage> StartProducing(CancellationToken token, TimeSpan interval)
        {
            var messages = new BlockingCollection<RawBusMessage>(100000);
            Task.Factory.StartNew(() => StartProducingToCollection(token, interval, messages), TaskCreationOptions.LongRunning);

            return messages;
        }

        // I didn't use this, but you can also produce to a BlockingCollection and turn that into an observable
        private void StartProducingToCollection(CancellationToken token, TimeSpan interval, BlockingCollection<RawBusMessage> messages)
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

                if (_counter % 10000 == 0)
                    Console.WriteLine("COLLECTION: " + messages.Count);

                if (lastTicks < reading.Ticks)
                {
                    var posted = messages.TryAdd(message);
                    if (!posted && ShowMessages.PrintFullMessageCollection)
                        Console.WriteLine("Buffer full. Could not post");
                }

                lastTicks = reading.Ticks;

                Thread.Sleep(interval);
            }
        }
    

        private RawBusMessage Read()
        {
            _counter++;
            var reading = _dataBus.Read();
            if(ShowMessages.PrintReader)
                Console.WriteLine($"Read message {_counter} on thread {Thread.CurrentThread.ManagedThreadId}");

            var message = new RawBusMessage();
            message.Data = reading.Data;
            message.ReadingTime = new DateTime(reading.Ticks);
            message.Counter = _counter;

            return message;
        }
    }
}
