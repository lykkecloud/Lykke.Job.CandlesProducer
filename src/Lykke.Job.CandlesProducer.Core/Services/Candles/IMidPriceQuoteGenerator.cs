using System.Collections.Immutable;
using Lykke.Domain.Prices.Contracts;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface IMidPriceQuoteGenerator : IHaveState<IImmutableDictionary<string, IMarketState>>
    {
        IQuote TryGenerate(IQuote quote, int assetPairAccuracy);
    }
}