using System.Collections.Immutable;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.QuotesProducer.Contract;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface IMidPriceQuoteGenerator : IHaveState<IImmutableDictionary<string, IMarketState>>
    {
        QuoteMessage TryGenerate(QuoteMessage quote, int assetPairAccuracy);
    }
}
