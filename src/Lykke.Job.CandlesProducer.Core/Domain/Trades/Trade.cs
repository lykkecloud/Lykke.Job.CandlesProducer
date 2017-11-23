using System;

namespace Lykke.Job.CandlesProducer.Core.Domain.Trades
{
    public class Trade
    {
        public string AssetPair { get; }
        public DateTime Timestamp { get; }
        public TradeType Type { get; }
        public double Price { get; }
        public double Volume { get; }

        public Trade(string assetPair, DateTime timestamp, double price, double volume)
        {
            AssetPair = assetPair;
            Timestamp = timestamp;
            Type = volume > 0 ? TradeType.Buy : TradeType.Sell;
            Price = price;
            Volume = Math.Abs(volume);
        }
    }
}
