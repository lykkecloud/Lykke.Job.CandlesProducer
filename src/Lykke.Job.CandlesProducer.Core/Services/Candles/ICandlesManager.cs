using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Domain.Trades;
using Lykke.Job.QuotesProducer.Contract;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface ICandlesManager
    {
        Task ProcessQuoteAsync(QuoteMessage quote);
        Task ProcessTradeAsync(Trade trade);
    }
}
