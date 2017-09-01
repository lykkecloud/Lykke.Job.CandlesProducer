using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.AzureRepositories
{
    public class CandlesGeneratorSnapshotRepository : ISnapshotRepository<IImmutableDictionary<string, ICandle>>
    {
        private const string Container = "CandlesGeneratorSnapshot";
        private const string Key = "Singleton";

        private readonly IBlobStorage _storage;

        public CandlesGeneratorSnapshotRepository(IBlobStorage storage)
        {
            _storage = storage;
        }

        public async Task SaveAsync(IImmutableDictionary<string, ICandle> state)
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                var model = state.ToDictionary(i => i.Key, i => CandleEntity.Create(i.Value));
                var serializer = new JsonSerializer();

                serializer.Serialize(jsonWriter, model);

                await jsonWriter.FlushAsync();
                await streamWriter.FlushAsync();
                await stream.FlushAsync();

                stream.Seek(0, SeekOrigin.Begin);

                await _storage.SaveBlobAsync(Container, Key, stream);
            }
        }

        public async Task<IImmutableDictionary<string, ICandle>> TryGetAsync()
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
                var model = serializer.Deserialize<ImmutableDictionary<string, CandleEntity>>(jsonReader);

                return model.ToImmutableDictionary(i => i.Key, i => (ICandle) i.Value);
            }
        }
    }
}