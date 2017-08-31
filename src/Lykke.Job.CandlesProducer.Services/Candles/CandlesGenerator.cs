using Lykke.Domain.Prices;
using Lykke.Domain.Prices.Contracts;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandlesGenerator : ICandlesGenerator
    {
        public Candle GenerateCandle(IQuote quote, TimeInterval timeInterval, PriceType priceType)
        {
            var intervalTimestamp = quote.Timestamp.RoundTo(timeInterval);

            return new Candle
            {
                AssetPairId = quote.AssetPair,
                TimeInterval = timeInterval,
                PriceType = priceType,
                Timestamp = intervalTimestamp,
                Open = quote.Price,
                Close = quote.Price,
                Low = quote.Price,
                High = quote.Price
            };
        }
    }
}