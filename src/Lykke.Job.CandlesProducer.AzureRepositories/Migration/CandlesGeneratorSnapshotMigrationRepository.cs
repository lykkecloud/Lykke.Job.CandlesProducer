using System.Collections.Immutable;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.CandlesProducer.AzureRepositories.Legacy;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.AzureRepositories.Migration
{
    public class CandlesGeneratorSnapshotMigrationRepository : ISnapshotRepository<IImmutableDictionary<string, ICandle>>
    {
        private readonly LegacyCandlesGeneratorSnapshotRepository _legacyRepository;
        private readonly CandlesGeneratorSnapshotRepository _repository;

        public CandlesGeneratorSnapshotMigrationRepository(IBlobStorage storage)
        {
            _legacyRepository = new LegacyCandlesGeneratorSnapshotRepository(storage);
            _repository = new CandlesGeneratorSnapshotRepository(storage);
        }

        public Task SaveAsync(IImmutableDictionary<string, ICandle> state)
        {
            return _repository.SaveAsync(state);
        }

        public async Task<IImmutableDictionary<string, ICandle>> TryGetAsync()
        {
            return await _repository.TryGetAsync() ?? await _legacyRepository.TryGetAsync();
        }
    }
}
