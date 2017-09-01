using System.Threading.Tasks;

namespace Lykke.Job.CandlesProducer.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}