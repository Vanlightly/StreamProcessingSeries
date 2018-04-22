using InProcStreamProcessing.Rx.DataBus;
using InProcStreamProcessing.Rx.Decoders;
using InProcStreamProcessing.Rx.Feeds;
using InProcStreamProcessing.Rx.Persistence;
using InProcStreamProcessing.Shared.SensorConfiguration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InProcStreamProcessing.Rx
{
    class ShowMessages
    {
        public static bool PrintFullMessageCollection = true;
        public static bool PrintReader = true;
        public static bool PrintFileWriter = false;
        public static bool PrintDecoder = false;
        public static bool PrintRealTimeFeed = true;
        public static bool PrintDbPersist = true;
        public static bool PrintStatsFeed = false;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var pipeline = new ProcessingPipeline(new DataBusReader(),
                    new MessageFileWriter(),
                    new Decoder(new ConfigurationLoader()),
                    new RealTimePublisher(),
                    new StatsFeedPublisher(),
                    new DbPersister());
            var pipelineTask = Task.Run(async () => pipeline.StartPipelineAsync(cts.Token)).Unwrap();

            Console.WriteLine("Press any key to shutdown the pipeline");
            Console.ReadKey();

            cts.Cancel();
            pipelineTask.Wait();


            Task.Delay(2000).Wait();
            Console.WriteLine("Pipeline shutdown");
        }
    }
}
