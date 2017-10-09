using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.AzureRepositories.Legacy
{
    [Obsolete("Used only for snapshot migration")]
    public class LegacyMidPriceQuoteGeneratorSnapshotRepository : ISnapshotRepository<IImmutableDictionary<string, IMarketState>>
    {
        private const string Container = "MidPriceQuoteGeneratorSnapshot";
        private const string Key = "Singleton";

        private readonly IBlobStorage _storage;

        public LegacyMidPriceQuoteGeneratorSnapshotRepository(IBlobStorage storage)
        {
            _storage = storage;
        }

        public Task SaveAsync(IImmutableDictionary<string, IMarketState> state)
        {
            throw new NotImplementedException();
        }

        public async Task<IImmutableDictionary<string, IMarketState>> TryGetAsync()
        {
            if (!await _storage.HasBlobAsync(Container, Key))
            {
                return null;
            }

            using (var stream = await _storage.GetAsync(Container, Key))
            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                await stream.FlushAsync();

                stream.Seek(0, SeekOrigin.Begin);

                var serializer = new JsonSerializer();
                var model = serializer.Deserialize<Dictionary<string, LegacyMarketStateEntity>>(jsonReader);

                return model.ToImmutableDictionary(i => i.Key, i => (IMarketState) i.Value);
            }
        }
    }
}
