using InProcStreamProcessing.Shared.SensorConfiguration;
using InProcStreamProcessing.TplDataFlow.DataBus;
using InProcStreamProcessing.TplDataFlow.Decoders;
using InProcStreamProcessing.TplDataFlow.Feeds;
using InProcStreamProcessing.TplDataFlow.Messages;
using InProcStreamProcessing.TplDataFlow.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace InProcStreamProcessing.TplDataFlow
{
    public class RoutedMessage
    {
        public RoutedMessage(int routeKey, DecodedMessage message)
        {
            RouteKey = routeKey;
            Message = message;
        }

        public int RouteKey { get; set; }
        public DecodedMessage Message { get; set; }
    }

    public class RoutedBatch
    {
        public RoutedBatch(int routeKey, IEnumerable<DecodedMessage> messages)
        {
            RouteKey = routeKey;
            Messages = messages;
        }

        public int RouteKey { get; set; }
        public IEnumerable<DecodedMessage> Messages { get; set; }
    }

    public class ProcessingPipeline
    {
        private IDataBusReader _dataBusReader;
        private IMessageFileWriter _messageFileWriter;
        private IDecoder _decoder;
        private IRealTimePublisher _realTimeFeedPublisher;
        private IStatsFeedPublisher _statsFeedPublisher;
        private IDbPersister _dbPersister;

        public ProcessingPipeline(IDataBusReader dataBusReader,
            IMessageFileWriter messageFileWriter,
            IDecoder decoder,
            IRealTimePublisher realTimePublisher,
            IStatsFeedPublisher s3Uploader,
            IDbPersister dbPersister)
        {
            _dataBusReader = dataBusReader;
            _messageFileWriter = messageFileWriter;
            _decoder = decoder;
            _realTimeFeedPublisher = realTimePublisher;
            _statsFeedPublisher = s3Uploader;
            _dbPersister = dbPersister;
        }

        public async Task StartPipelineAsync(CancellationToken token)
        {
            _decoder.LoadSensorConfigs();

            // Step 1 - Configure the pipeline
            
            // make sure our complete call gets propagated throughout the whole pipeline
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            // create our block configurations
            var largeBufferOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 600000 };
            var smallBufferOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 1000 };
            var realTimeBufferOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 6000 };
            var parallelizedOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 1000, MaxDegreeOfParallelism = 4 };
            var batchOptions = new GroupingDataflowBlockOptions() { BoundedCapacity = 1000 };

            // define each block
            var writeRawMessageBlock = new TransformBlock<RawBusMessage, RawBusMessage>(async (RawBusMessage msg) =>
            {
                await _messageFileWriter.WriteAsync(msg);
                return msg;
            }, largeBufferOptions);

            var decoderBlock = new TransformManyBlock<RawBusMessage, DecodedMessage>(
                (RawBusMessage msg) => _decoder.Decode(msg), largeBufferOptions);

            var broadcast = new BroadcastBlock<DecodedMessage>(msg => msg);

            var realTimeFeedBlock = new ActionBlock<DecodedMessage>(async 
                (DecodedMessage msg) => await _realTimeFeedPublisher.PublishAsync(msg), realTimeBufferOptions);

            var oneSecondBatchBlock = new BatchBlock<DecodedMessage>(3000);
            var thirtySecondBatchBlock = new BatchBlock<DecodedMessage>(90000);
            var batchBroadcastBlock = new BroadcastBlock<DecodedMessage[]>(msg => msg);

            var oneSecondStatsFeedBlock = new ActionBlock<DecodedMessage[]>(async 
                (DecodedMessage[] messages) => await _statsFeedPublisher.PublishAsync(messages.ToList(), TimeSpan.FromSeconds(1)), smallBufferOptions);

            var dbPersistenceBlock = new ActionBlock<DecodedMessage[]>(async 
                (DecodedMessage[] messages) => await _dbPersister.PersistAsync(messages.ToList()), smallBufferOptions);
            var thirtySecondStatsFeedBlock = new ActionBlock<DecodedMessage[]>(async 
                (DecodedMessage[] messages) => await _statsFeedPublisher.PublishAsync(messages.ToList(), TimeSpan.FromSeconds(30)), smallBufferOptions);

            // link the blocks to together
            writeRawMessageBlock.LinkTo(decoderBlock, linkOptions);
            decoderBlock.LinkTo(broadcast, linkOptions);
            broadcast.LinkTo(realTimeFeedBlock, linkOptions);
            broadcast.LinkTo(oneSecondBatchBlock, linkOptions);
            broadcast.LinkTo(thirtySecondBatchBlock, linkOptions);
            oneSecondBatchBlock.LinkTo(batchBroadcastBlock, linkOptions);
            batchBroadcastBlock.LinkTo(oneSecondStatsFeedBlock, linkOptions);
            batchBroadcastBlock.LinkTo(dbPersistenceBlock, linkOptions);
            thirtySecondBatchBlock.LinkTo(thirtySecondStatsFeedBlock, linkOptions);

            // Step 2 - Start consuming the machine bus interface (the producer)
            var consumerTask = _dataBusReader.StartConsuming(writeRawMessageBlock, token, TimeSpan.FromMilliseconds(1000), FlowControlMode.LoadShed);

            // Step 3 - Keep going until the CancellationToken is cancelled.
            while(!token.IsCancellationRequested)
                await Task.Delay(500);

            // Step 4 - the CancellationToken has been cancelled and our producer has stopped producing
            // call Complete on the first block, this will propagate down the pipeline
            writeRawMessageBlock.Complete();

            // wait for all leaf blocks to finish processing their data
            await realTimeFeedBlock.Completion;
            await oneSecondStatsFeedBlock.Completion;
            await dbPersistenceBlock.Completion;
            await thirtySecondStatsFeedBlock.Completion;
            await consumerTask;

            // clean up any other resources like ZeroMQ for example
        }

        public async Task StartPipelineWithBackPressureAsync(CancellationToken token)
        {
            _decoder.LoadSensorConfigs();

            // Step 1 - Configure the pipeline

            // make sure our complete call gets propagated throughout the whole pipeline
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            // create our block configurations
            var largeBufferOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 60000 };
            var smallBufferOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 1000 };
            var parallelizedOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 1000, MaxDegreeOfParallelism = 4 };
            var batchOptions = new GroupingDataflowBlockOptions() { BoundedCapacity = 1000 };

            // create some branching functions for our TransformManyBlocks
            // DecodedMessage gets tranformed into 3 RoutedMessage for the first three way branch
            Func<DecodedMessage, IEnumerable<RoutedMessage>> messageBranchFunc = x => new List<RoutedMessage>
                {
                    new RoutedMessage(1, x),
                    new RoutedMessage(2, x),
                    new RoutedMessage(3, x)
                };

            // DecodedMessage[] gets tranformed into a RoutedBatch for the final branch
            Func<RoutedMessage[], IEnumerable<RoutedBatch>> batchBranchFunc = x => new List<RoutedBatch>
                {
                    new RoutedBatch(1, x.Select(c => c.Message).ToList()),
                    new RoutedBatch(2, x.Select(c => c.Message).ToList())
                };

            // define each block
            var writeRawMessageBlock = new TransformBlock<RawBusMessage, RawBusMessage>(async (RawBusMessage msg) =>
            {
                await _messageFileWriter.WriteAsync(msg);
                return msg;
            }, largeBufferOptions);

            var decoderBlock = new TransformManyBlock<RawBusMessage, DecodedMessage>(
                (RawBusMessage msg) => _decoder.Decode(msg), largeBufferOptions);


            var msgBranchBlock = new TransformManyBlock<DecodedMessage, RoutedMessage>(messageBranchFunc, largeBufferOptions);

            var realTimeFeedBlock = new ActionBlock<RoutedMessage>(async (RoutedMessage routedMsg) => 
                    await _realTimeFeedPublisher.PublishAsync(routedMsg.Message), largeBufferOptions);

            var thirtySecondBatchBlock = new BatchBlock<RoutedMessage>(90000, batchOptions);
            var thirtySecondStatsFeedBlock = new ActionBlock<RoutedMessage[]>(async (RoutedMessage[] batch) =>
                    await _statsFeedPublisher.PublishAsync(batch.Select(x => x.Message).ToList(), TimeSpan.FromSeconds(30)), smallBufferOptions);

            var oneSecondBatchBlock = new BatchBlock<RoutedMessage>(3000, batchOptions);
            var batchBroadcastBlock = new TransformManyBlock<RoutedMessage[], RoutedBatch>(batchBranchFunc, smallBufferOptions);

            var oneSecondStatsFeedBlock = new ActionBlock<RoutedBatch>(async (RoutedBatch batch) =>
                    await _statsFeedPublisher.PublishAsync(batch.Messages.ToList(), TimeSpan.FromSeconds(1)), smallBufferOptions);

            var dbPersistenceBlock = new ActionBlock<RoutedBatch>(async (RoutedBatch batch) => 
                    await _dbPersister.PersistAsync(batch.Messages.ToList()), parallelizedOptions);

            // link the blocks to together
            writeRawMessageBlock.LinkTo(decoderBlock, linkOptions);
            decoderBlock.LinkTo(msgBranchBlock, linkOptions);
            msgBranchBlock.LinkTo(realTimeFeedBlock, routedMsg => routedMsg.RouteKey == 1); // route on the key
            msgBranchBlock.LinkTo(oneSecondBatchBlock, routedMsg => routedMsg.RouteKey == 2); // route on the key
            msgBranchBlock.LinkTo(thirtySecondBatchBlock, routedMsg => routedMsg.RouteKey == 3); // route on the key
            thirtySecondBatchBlock.LinkTo(thirtySecondStatsFeedBlock, linkOptions);
            oneSecondBatchBlock.LinkTo(batchBroadcastBlock, linkOptions);
            batchBroadcastBlock.LinkTo(oneSecondStatsFeedBlock, routedMsg => routedMsg.RouteKey == 1); // route on the key
            batchBroadcastBlock.LinkTo(dbPersistenceBlock, routedMsg => routedMsg.RouteKey == 2); // route on the key

            // Step 2 - Start consuming the machine bus interface (the producer)
            var consumerTask = _dataBusReader.StartConsuming(writeRawMessageBlock, token, TimeSpan.FromMilliseconds(1000), FlowControlMode.BackPressure);

            // Step 3 - Keep going until the CancellationToken is cancelled.
            while (!token.IsCancellationRequested)
                await Task.Delay(500);

            // Step 4 - the CancellationToken has been cancelled and our producer has stopped producing
            // call Complete on the first block, this will propagate down the pipeline
            writeRawMessageBlock.Complete();

            // wait for all leaf blocks to finish processing their data
            await oneSecondStatsFeedBlock.Completion;
            await thirtySecondStatsFeedBlock.Completion;
            await dbPersistenceBlock.Completion;
            await realTimeFeedBlock.Completion;
            await consumerTask;

            // clean up any other resources like ZeroMQ for example
        }
    }
}
