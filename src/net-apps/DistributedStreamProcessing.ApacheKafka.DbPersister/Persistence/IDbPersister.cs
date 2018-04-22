using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedStreamProcessing.ApacheKafka.DbPersister.Persistence
{
    public interface IDbPersister
    {
        void StartPersisting(CancellationToken token, TimeSpan interval, BlockingCollection<DecodedMessage> decodedMessages);
    }
}
