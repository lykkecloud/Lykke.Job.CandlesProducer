using System.Collections.Immutable;
using Lykke.Domain.Prices;
using Lykke.Domain.Prices.Contracts;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface ICandlesGenerator : IHaveState<IImmutableDictionary<string, ICandle>>
    {
        CandleMergeResult Merge(IQuote quote, PriceType priceType, TimeInterval timeInterval);
    }
}