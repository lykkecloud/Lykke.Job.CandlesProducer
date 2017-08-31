using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.AzureRepositories
{
    public class MidPriceQuoteGeneratorSnapshotRepository : IMidPriceQuoteGeneratorSnapshotRepository
    {
        private const string Container = "MidPriceQuoteGeneratorSnapshot";
        private const string Key = "Singleton";

        private readonly IBlobStorage _storage;

        public MidPriceQuoteGeneratorSnapshotRepository(IBlobStorage storage)
        {
            _storage = storage;
        }

        public async Task SaveAsync(IEnumerable<KeyValuePair<string, IMarketState>> state)
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                var serializer = new JsonSerializer();
                var model = state.ToDictionary(i => i.Key, i => MarketStateEntity.Create(i.Value));

                serializer.Serialize(jsonWriter, model);

                await jsonWriter.FlushAsync();
                await streamWriter.FlushAsync();
                await stream.FlushAsync();

                stream.Seek(0, SeekOrigin.Begin);

                await _storage.SaveBlobAsync(Container, Key, stream);
            }
        }

        public async Task<IEnumerable<KeyValuePair<string, IMarketState>>> TryGetAsync()
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
                var model = serializer.Deserialize<Dictionary<string, MarketStateEntity>>(jsonReader);
                
                return model.Select(i => KeyValuePair.Create<string, IMarketState>(i.Key, i.Value));
            }
        }
    }
}