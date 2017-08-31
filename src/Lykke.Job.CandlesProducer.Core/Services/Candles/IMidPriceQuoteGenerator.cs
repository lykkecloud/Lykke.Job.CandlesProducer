using System.Collections.Generic;
using Lykke.Domain.Prices.Contracts;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface IMidPriceQuoteGenerator
    {
        IQuote TryGenerate(IQuote quote, int assetPairAccuracy);
        IEnumerable<KeyValuePair<string, IMarketState>> GetState();
        void SetState(IEnumerable<KeyValuePair<string, IMarketState>> state);
    }
}