using System.Threading.Tasks;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}