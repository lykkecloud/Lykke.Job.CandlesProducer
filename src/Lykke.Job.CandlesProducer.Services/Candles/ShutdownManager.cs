using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly IQuotesSubscriber _quotesSubscriber;
        private readonly ICandlesPublisher _publisher;
        private readonly IMidPriceQuoteGeneratorSnapshotSerializer _midPriceQuoteGeneratorSnapshotSerializer;
        private readonly ILog _log;

        public ShutdownManager(
            IQuotesSubscriber quotesSubscriber,
            ICandlesPublisher publisher,
            IMidPriceQuoteGeneratorSnapshotSerializer midPriceQuoteGeneratorSnapshotSerializer,
            ILog log)
        {
            _quotesSubscriber = quotesSubscriber;
            _publisher = publisher;
            _midPriceQuoteGeneratorSnapshotSerializer = midPriceQuoteGeneratorSnapshotSerializer;
            _log = log;
        }

        public async Task ShutdownAsync()
        {
            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(ShutdownAsync), "", "Stopping quotes subscriber...");

            _quotesSubscriber.Stop();

            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(ShutdownAsync), "", "Serializing mid price quote generator snapshot async...");
            
            var serializeTask = _midPriceQuoteGeneratorSnapshotSerializer.SerializeAsync();

            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(ShutdownAsync), "", "Stopping candles publisher...");

            _publisher.Stop();

            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(ShutdownAsync), "", "Awaiting for mid price quote generator snapshot serialization...");
            
            await serializeTask;

            await _log.WriteInfoAsync(nameof(ShutdownManager), nameof(ShutdownAsync), "", "Shutted down");
        }
    }
}