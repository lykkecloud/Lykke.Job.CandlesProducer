using System.Collections.Immutable;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.CandlesProducer.AzureRepositories.Legacy;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.AzureRepositories.Migration
{
    public class MidPriceQuoteGeneratorSnapshotMigrationRepository : ISnapshotRepository<IImmutableDictionary<string, IMarketState>>
    {
        private readonly LegacyMidPriceQuoteGeneratorSnapshotRepository _legacyRepository;
        private readonly MidPriceQuoteGeneratorSnapshotRepository _repository;

        public MidPriceQuoteGeneratorSnapshotMigrationRepository(IBlobStorage storage)
        {
            _legacyRepository = new LegacyMidPriceQuoteGeneratorSnapshotRepository(storage);
            _repository = new MidPriceQuoteGeneratorSnapshotRepository(storage);
        }

        public Task SaveAsync(IImmutableDictionary<string, IMarketState> state)
        {
            return _repository.SaveAsync(state);
        }

        public async Task<IImmutableDictionary<string, IMarketState>> TryGetAsync()
        {
            return await _repository.TryGetAsync() ?? await _legacyRepository.TryGetAsync();
        }
    }
}
