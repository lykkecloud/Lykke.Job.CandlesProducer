using Lykke.Domain.Prices;
using Lykke.Domain.Prices.Contracts;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface ICandlesGenerator
    {
        Candle GenerateCandle(IQuote quote, TimeInterval timeInterval, PriceType priceType);
    }
}