using System.Collections.Immutable;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using Lykke.Job.CandlesProducer.AzureRepositories.Legacy;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.AzureRepositories.Migration
{
    public class CandlesGeneratorSnapshotMigrationRepository : ISnapshotRepository<IImmutableDictionary<string, ICandle>>
    {
        private readonly ILog _log;
        private readonly LegacyCandlesGeneratorSnapshotRepository _legacyRepository;
        private readonly CandlesGeneratorSnapshotRepository _repository;

        public CandlesGeneratorSnapshotMigrationRepository(IBlobStorage storage, ILog log)
        {
            _log = log;
            _legacyRepository = new LegacyCandlesGeneratorSnapshotRepository(storage);
            _repository = new CandlesGeneratorSnapshotRepository(storage);
        }

        public Task SaveAsync(IImmutableDictionary<string, ICandle> state)
        {
            return _repository.SaveAsync(state);
        }

        public async Task<IImmutableDictionary<string, ICandle>> TryGetAsync()
        {
            var newResult = await _repository.TryGetAsync();
            if (newResult == null)
            {
                await _log.WriteWarningAsync(nameof(CandlesGeneratorSnapshotMigrationRepository), nameof(TryGetAsync), "",
                    "Failed to get snapshot in the new format, fallback to the legacy format");

                return await _legacyRepository.TryGetAsync();
            }

            return newResult;
        }
    }
}
