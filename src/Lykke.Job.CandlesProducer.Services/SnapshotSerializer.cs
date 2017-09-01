using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services
{
    public class SnapshotSerializer<TState> : ISnapshotSerializer
    {
        private readonly IHaveState<TState> _stateHolder;
        private readonly ISnapshotRepository<TState> _repository;
        private readonly ILog _log;

        public SnapshotSerializer(
            IHaveState<TState> stateHolder,
            ISnapshotRepository<TState> repository,
            ILog log)
        {
            _stateHolder = stateHolder;
            _repository = repository;
            _log = log;
        }

        public Task SerializeAsync()
        {
            var state = _stateHolder.GetState();

            return _repository.SaveAsync(state);
        }

        public async Task DeserializeAsync()
        {
            var state = await _repository.TryGetAsync();

            if (state == null)
            {
                await _log.WriteWarningAsync("SnapshotSerializer", nameof(DeserializeAsync),
                    _stateHolder.GetType().Name, "No snapshot found to deserialize");

                return;
            }

            _stateHolder.SetState(state);
        }
    }
}