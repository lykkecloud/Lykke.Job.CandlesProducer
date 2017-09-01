using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly IQuotesSubscriber _quotesSubscriber;
        private readonly IEnumerable<ISnapshotSerializer> _snapshotSerializers;
        private readonly ILog _log;

        public StartupManager(
            IQuotesSubscriber quotesSubscriber,
            IEnumerable<ISnapshotSerializer> snapshotSerializers,
            ILog log)
        {
            _quotesSubscriber = quotesSubscriber;
            _snapshotSerializers = snapshotSerializers;
            _log = log;
        }

        public  async Task StartAsync()
        {
            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "", "Deserializing snapshots...");

            await Task.WhenAll(_snapshotSerializers.Select(s => s.DeserializeAsync()));

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "", "Starting quote subscriber...");

            _quotesSubscriber.Start();

            await _log.WriteInfoAsync(nameof(StartupManager), nameof(StartAsync), "", "Started up");
        }
    }
}