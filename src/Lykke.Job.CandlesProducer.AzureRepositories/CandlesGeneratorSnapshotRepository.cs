using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using MessagePack;

namespace Lykke.Job.CandlesProducer.AzureRepositories
{
    public class CandlesGeneratorSnapshotRepository : ISnapshotRepository<IImmutableDictionary<string, ICandle>>
    {
        private const string Key = "CandlesGenerator";

        private readonly IBlobStorage _storage;

        public CandlesGeneratorSnapshotRepository(IBlobStorage storage)
        {
            _storage = storage;
        }

        public async Task SaveAsync(IImmutableDictionary<string, ICandle> state)
        {
            using (var stream = new MemoryStream())
            {
                var model = state.ToDictionary(i => i.Key, i => CandleEntity.Create(i.Value));

                MessagePackSerializer.Serialize(stream, model);

                await stream.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);

                await _storage.SaveBlobAsync(Constants.SnapshotsContainer, Key, stream);
            }
        }

        public async Task<IImmutableDictionary<string, ICandle>> TryGetAsync()
        {
            if (!await _storage.HasBlobAsync(Constants.SnapshotsContainer, Key))
            {
                return null;
            }

            using (var stream = await _storage.GetAsync(Constants.SnapshotsContainer, Key))
            {
                var model = MessagePackSerializer.Deserialize<Dictionary<string, CandleEntity>>(stream);

                return model.ToImmutableDictionary(i => i.Key, i => (ICandle) i.Value);
            }
        }
    }
}
