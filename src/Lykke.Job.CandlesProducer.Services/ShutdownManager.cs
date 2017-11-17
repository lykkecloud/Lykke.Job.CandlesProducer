using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Quotes;

namespace Lykke.Job.CandlesProducer.Services
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly IQuotesSubscriber _quotesSubscriber;
        private readonly ICandlesPublisher _publisher;
        private readonly IEnumerable<ISnapshotSerializer> _snapshotSerializers;
        private readonly ILog _log;

        public ShutdownManager(
            IQuotesSubscriber quotesSubscriber,
            ICandlesPublisher publisher,
            IEnumerable<ISnapshotSerializer> snapshotSerializerses,
            ILog log)
        {
            _quotesSubscriber = quotesSubscriber;
            _publisher = publisher;
            _snapshotSerializers = snapshotSerializerses;
            _log = log;
        }

        public async Task ShutdownAsync()
        {
            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(ShutdownAsync), "", "Stopping quotes subscriber...");

            _quotesSubscriber.Stop();

            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(ShutdownAsync), "", "Serializing snapshots async...");
            
            var snapshotSrializationTasks = _snapshotSerializers.Select(s  => s.SerializeAsync());

            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(ShutdownAsync), "", "Stopping candles publisher...");

            _publisher.Stop();

            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(ShutdownAsync), "", "Awaiting for snapshots serialization...");

            await Task.WhenAll(snapshotSrializationTasks);

            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(ShutdownAsync), "", "Shutted down");
        }
    }
}
