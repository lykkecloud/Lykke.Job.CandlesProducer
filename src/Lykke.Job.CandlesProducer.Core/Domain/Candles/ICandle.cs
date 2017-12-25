using Lykke.Job.CandlesProducer.Contract;
using System;

namespace Lykke.Job.CandlesProducer.Core.Domain.Candles
{
    public interface ICandle
    {
        string AssetPairId { get; }
        CandlePriceType PriceType { get; }
        CandleTimeInterval TimeInterval { get; }
        DateTime Timestamp { get; }
        double Open { get; }
        double Close { get; }
        double High { get; }
        double Low { get; }
        double TradingVolume { get; }
        double LastTradePrice { get; }
        DateTime LatestChangeTimestamp { get; }
        DateTime OpenTimestamp { get; }
        bool HasPrices { get; }
    }
}
