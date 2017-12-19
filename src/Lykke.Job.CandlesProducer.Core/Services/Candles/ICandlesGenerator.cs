using System;
using System.Collections.Immutable;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface ICandlesGenerator : IHaveState<ImmutableDictionary<string, ImmutableList<ICandle>>>
    {
        CandleUpdateResult UpdatePrice(string assetPair, DateTime timestamp, double price, CandlePriceType priceType, CandleTimeInterval timeInterval);
        CandleUpdateResult UpdateTradingVolume(string assetPair, DateTime timestamp, double volume, double tradePrice, CandlePriceType priceType, CandleTimeInterval timeInterval);
        void Undo(CandleUpdateResult candleUpdateResult);
    }
}
