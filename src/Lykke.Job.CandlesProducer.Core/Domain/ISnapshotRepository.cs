using System.Threading.Tasks;

namespace Lykke.Job.CandlesProducer.Core.Domain
{
    public interface ISnapshotRepository<TState>
    {
        Task SaveAsync(TState state);
        Task<TState> TryGetAsync();
    }
}