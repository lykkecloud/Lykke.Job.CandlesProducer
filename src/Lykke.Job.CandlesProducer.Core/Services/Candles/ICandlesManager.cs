using System.Threading.Tasks;
using Lykke.Domain.Prices.Contracts;
using Lykke.Job.CandlesProducer.Core.Domain.Trades;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface ICandlesManager
    {
        Task ProcessQuoteAsync(IQuote quote);
        Task ProcessTradeAsync(Trade trade);
    }
}
