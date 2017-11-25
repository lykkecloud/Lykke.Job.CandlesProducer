using System;
using System.Collections.Immutable;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface ICandlesGenerator : IHaveState<ImmutableDictionary<string, ImmutableList<ICandle>>>
    {
        CandleUpdateResult Update(string assetPair, DateTime timestamp, double price, double volume, CandlePriceType priceType, CandleTimeInterval timeInterval);
        void Undo(CandleUpdateResult candleUpdateResult);
    }
}
