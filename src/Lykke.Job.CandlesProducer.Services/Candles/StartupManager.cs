using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class StartupManager : IStartupManager
    {
        private readonly IQuotesSubscriber _quotesSubscriber;
        private readonly IMidPriceQuoteGeneratorSnapshotSerializer _midPriceQuoteGeneratorSnapshotSerializer;
        private readonly ILog _log;

        public StartupManager(
            IQuotesSubscriber quotesSubscriber,
            IMidPriceQuoteGeneratorSnapshotSerializer midPriceQuoteGeneratorSnapshotSerializer,
            ILog log)
        {
            _quotesSubscriber = quotesSubscriber;
            _midPriceQuoteGeneratorSnapshotSerializer = midPriceQuoteGeneratorSnapshotSerializer;
            _log = log;
        }

        public  async Task StartAsync()
        {
            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "", "Deserializing mid price quote generator snapshot...");

            await _midPriceQuoteGeneratorSnapshotSerializer.DeserializeAsync();

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "", "Starting quote subscriber...");

            _quotesSubscriber.Start();

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "", "Started up");
        }
    }
}