using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class MidPriceQuoteGeneratorSnapshotSerializer : IMidPriceQuoteGeneratorSnapshotSerializer
    {
        private readonly IMidPriceQuoteGenerator _generator;
        private readonly IMidPriceQuoteGeneratorSnapshotRepository _repository;
        private readonly ILog _log;

        public MidPriceQuoteGeneratorSnapshotSerializer(
            IMidPriceQuoteGenerator generator,
            IMidPriceQuoteGeneratorSnapshotRepository repository,
            ILog log)
        {
            _generator = generator;
            _repository = repository;
            _log = log;
        }
        public Task SerializeAsync()
        {
            var state = _generator.GetState();

            return _repository.SaveAsync(state);
        }

        public async Task DeserializeAsync()
        {
            var state = await _repository.TryGetAsync();

            if (state == null)
            {
                await _log.WriteWarningAsync(nameof(MidPriceQuoteGeneratorSnapshotSerializer), nameof(DeserializeAsync),
                    "", "No mid price quote generator snapshot found to deserialize");

                return;
            }

            _generator.SetState(state);
        }
    }
}