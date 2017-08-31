using System.Threading.Tasks;
using Lykke.Domain.Prices.Contracts;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface ICandlesManager
    {
        Task ProcessQuoteAsync(IQuote quote);
    }
}