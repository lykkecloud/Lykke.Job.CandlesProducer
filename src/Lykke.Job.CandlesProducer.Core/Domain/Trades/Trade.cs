using System;

namespace Lykke.Job.CandlesProducer.Core.Domain.Trades
{
    public class Trade
    {
        public string AssetPair { get; }
        public DateTime Timestamp { get; }
        public TradeType Type { get; }
        public double Volume { get; }
        public double Price { get; set; }

        public Trade(string assetPair, TradeType type, DateTime timestamp, double volume, double price)
        {
            AssetPair = assetPair;
            Timestamp = timestamp;
            Type = type;
            Volume = Math.Abs(volume);
            Price = price;
        }
    }
}
