using System.Threading.Tasks;

namespace Lykke.Job.CandlesProducer.Core.Services
{
    public interface ISnapshotSerializer
    {
        Task SerializeAsync();
        Task DeserializeAsync();
    }
}