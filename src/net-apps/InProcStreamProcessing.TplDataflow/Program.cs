using InProcStreamProcessing.Shared.SensorConfiguration;
using InProcStreamProcessing.TplDataFlow.DataBus;
using InProcStreamProcessing.TplDataFlow.Decoders;
using InProcStreamProcessing.TplDataFlow.Feeds;
using InProcStreamProcessing.TplDataFlow.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InProcStreamProcessing.TplDataFlow
{
    class ShowMessages
    {
        public static bool PrintReader = true;
        public static bool PrintFileWriter = true;
        public static bool PrintDecoder = true;
        public static bool PrintRealTimeFeed = true;
        public static bool PrintDbPersist = true;
        public static bool PrintStatsFeed = true;
        public static bool PrintFullBuffers = true;
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var cts = new CancellationTokenSource();
                var pipeline = new ProcessingPipeline(new DataBusReader(),
                    new MessageFileWriter(),
                    new Decoder(new ConfigurationLoader()),
                    new RealTimePublisher(),
                    new StatsFeedPublisher(),
                    new DbPersister());

                var pipelineTask = Task.Run(async () => {
                    try
                    {
                        await pipeline.StartPipelineWithBackPressureAsync(cts.Token);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Pipeline terminated due to error {ex}");
                    }
                });

                Console.WriteLine("Press any key to shutdown the pipeline");
                Console.ReadKey();

                cts.Cancel();
                pipelineTask.Wait();

                Console.WriteLine("Pipeline shutdown");
            }
            catch (Exception ex)
            {

            }
        }
    }
}
