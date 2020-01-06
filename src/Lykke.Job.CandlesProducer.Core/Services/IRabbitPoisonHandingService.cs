using System.Threading.Tasks;

namespace Lykke.Job.CandlesProducer.Core.Services
{
    public interface IRabbitPoisonHandingService<T> where T : class
    {
        Task<string> PutMessagesBack();
    }
}
