using System;
using Lykke.Job.CandlesProducer.Contract;

namespace Lykke.Job.CandlesProducer.Core.Domain.Candles
{
    public class Candle : IEquatable<Candle>, ICandle
    {
        public string AssetPairId { get; }
        public CandlePriceType PriceType { get; }
        public CandleTimeInterval TimeInterval { get; }
        public DateTime Timestamp { get; }
        public double Open { get; }
        public double Close { get; }
        public double High { get; }
        public double Low { get; }
        public double TradingVolume { get; }
        public DateTime LatestChangeTimestamp { get; }
        public DateTime OpenTimestamp { get; }

        private Candle(
            string assetPairId, 
            CandlePriceType priceType, 
            CandleTimeInterval timeInterval, 
            DateTime timestamp, 
            DateTime latestChangeTimestamp,
            DateTime openTimestamp,
            double open, 
            double close, 
            double low, 
            double high,
            double tradingVolume)
        {
            AssetPairId = assetPairId;
            PriceType = priceType;
            TimeInterval = timeInterval;
            Timestamp = timestamp;
            LatestChangeTimestamp = latestChangeTimestamp;
            OpenTimestamp = openTimestamp;
            Open = open;
            Close = close;
            Low = low;
            High = high;
            TradingVolume = tradingVolume;
        }

        public static Candle Copy(ICandle candle)
        {
            return new Candle
            (
                candle.AssetPairId,
                candle.PriceType,
                candle.TimeInterval,
                candle.Timestamp,
                candle.LatestChangeTimestamp,
                candle.OpenTimestamp,
                candle.Open,
                candle.Close,
                candle.Low,
                candle.High,
                candle.TradingVolume
            );
        }

        public static Candle Create(
            string assetPair,
            DateTime timestamp,
            double price, 
            double tradingVolume, 
            CandlePriceType priceType, 
            CandleTimeInterval timeInterval)
        {
            var intervalTimestamp = timestamp.TruncateTo(timeInterval);

            return new Candle
            (
                assetPair,
                priceType,
                timeInterval,
                intervalTimestamp,
                timestamp,
                timestamp,
                price,
                price,
                price,
                price,
                tradingVolume
            );
        }

        public Candle Update(DateTime timestamp, double price, double tradingVolume)
        {
            double closePrice;
            double openPrice;
            DateTime changeTimestamp;
            DateTime openTimestamp;

            if (LatestChangeTimestamp < timestamp)
            {
                closePrice = price;
                changeTimestamp = timestamp;
            }
            else
            {
                closePrice = Close;
                changeTimestamp = LatestChangeTimestamp;
            }

            if (OpenTimestamp > timestamp)
            {
                openPrice = price;
                openTimestamp = timestamp;
            }
            else
            {
                openPrice = Open;
                openTimestamp = OpenTimestamp;
            }

            return new Candle(
                AssetPairId,
                PriceType,
                TimeInterval,
                Timestamp,
                changeTimestamp,
                openTimestamp,
                openPrice,
                closePrice,
                Math.Min(Low, price),
                Math.Max(High, price),
                TradingVolume + tradingVolume);
        }

        public Candle SubstractVolume(double volume)
        {
            return new Candle(
                AssetPairId,
                PriceType,
                TimeInterval,
                Timestamp,
                LatestChangeTimestamp,
                OpenTimestamp,
                Open,
                Close,
                Low,
                High,
                TradingVolume - volume);
        }

        public bool Equals(Candle other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(AssetPairId, other.AssetPairId) &&
                   PriceType == other.PriceType &&
                   TimeInterval == other.TimeInterval &&
                   Timestamp.Equals(other.Timestamp) &&
                   LatestChangeTimestamp.Equals(other.LatestChangeTimestamp) &&
                   Open.Equals(other.Open) &&
                   Close.Equals(other.Close) &&
                   High.Equals(other.High) &&
                   Low.Equals(other.Low) &&
                   TradingVolume.Equals(other.TradingVolume);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Candle)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = AssetPairId != null ? AssetPairId.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (int)PriceType;
                hashCode = (hashCode * 397) ^ (int)TimeInterval;
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ LatestChangeTimestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ Open.GetHashCode();
                hashCode = (hashCode * 397) ^ Close.GetHashCode();
                hashCode = (hashCode * 397) ^ High.GetHashCode();
                hashCode = (hashCode * 397) ^ Low.GetHashCode();
                hashCode = (hashCode * 397) ^ TradingVolume.GetHashCode();

                return hashCode;
            }
        }
    }
}
