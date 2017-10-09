using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Lykke.Domain.Prices;
using Lykke.Domain.Prices.Contracts;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandlesGenerator : ICandlesGenerator
    {
        private class Candle : IEquatable<Candle>, ICandle
        {
            [JsonProperty("a")]
            public string AssetPairId { get; }
            [JsonProperty("p")]
            public PriceType PriceType { get; }
            [JsonProperty("i")]
            public TimeInterval TimeInterval { get; }
            [JsonProperty("t")]
            public DateTime Timestamp { get; }
            [JsonProperty("o")]
            public double Open { get; }
            [JsonProperty("c")]
            public double Close { get; }
            [JsonProperty("h")]
            public double High { get; }
            [JsonProperty("l")]
            public double Low { get; }

            private Candle(string assetPairId, PriceType priceType, TimeInterval timeInterval, DateTime timestamp, double open, double close, double low, double high)
            {
                AssetPairId = assetPairId;
                PriceType = priceType;
                TimeInterval = timeInterval;
                Timestamp = timestamp;
                Open = open;
                Close = close;
                Low = low;
                High = high;
            }

            public static Candle Create(ICandle candle)
            {
                return new Candle
                (
                    candle.AssetPairId,
                    candle.PriceType,
                    candle.TimeInterval,
                    candle.Timestamp,
                    candle.Open,
                    candle.Close,
                    candle.Low,
                    candle.High
                );
            }

            public static Candle Create(IQuote quote, PriceType priceType, TimeInterval timeInterval)
            {
                var intervalTimestamp = quote.Timestamp.RoundTo(timeInterval);

                return new Candle
                (
                    quote.AssetPair,
                    priceType,
                    timeInterval,
                    intervalTimestamp,
                    quote.Price,
                    quote.Price,
                    quote.Price,
                    quote.Price
                );
            }

            public static Candle Create(ICandle oldState, IQuote quote)
            {
                var intervalTimestamp = quote.Timestamp.RoundTo(oldState.TimeInterval);

                // Start new candle?
                if (intervalTimestamp != oldState.Timestamp)
                {
                    return Create(quote, oldState.PriceType, oldState.TimeInterval);
                }

                // Merge oldState with new quote
                return new Candle(
                    oldState.AssetPairId,
                    oldState.PriceType,
                    oldState.TimeInterval,
                    intervalTimestamp,
                    oldState.Open,
                    quote.Price,
                    Math.Min(oldState.Low, quote.Price),
                    Math.Max(oldState.Low, quote.Price));
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
                       Open.Equals(other.Open) &&
                       Close.Equals(other.Close) &&
                       High.Equals(other.High) &&
                       Low.Equals(other.Low);
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
                    hashCode = (hashCode * 397) ^ Open.GetHashCode();
                    hashCode = (hashCode * 397) ^ Close.GetHashCode();
                    hashCode = (hashCode * 397) ^ High.GetHashCode();
                    hashCode = (hashCode * 397) ^ Low.GetHashCode();

                    return hashCode;
                }
            }
        }

        private Dictionary<string, Candle> _candles;

        public CandlesGenerator()
        {
            _candles = new Dictionary<string, Candle>();
        }

        public CandleMergeResult Merge(IQuote quote, PriceType priceType, TimeInterval timeInterval)
        {
            var key = GetKey(quote.AssetPair, timeInterval, priceType);
            var newCandle = _candles.TryGetValue(key, out Candle oldCandle) ? 
                Candle.Create(oldCandle, quote) : 
                Candle.Create(quote, priceType, timeInterval);

            _candles[key] = newCandle;

            return new CandleMergeResult(newCandle, !newCandle.Equals(oldCandle));
        }

        public IImmutableDictionary<string, ICandle> GetState()
        {
            return _candles.ToImmutableDictionary(i => i.Key, i => (ICandle)i.Value);
        }

        public void SetState(IImmutableDictionary<string, ICandle> state)
        {
            if (_candles.Count > 0)
            {
                throw new InvalidOperationException("Candles generator state already not empty");
            }

            _candles = state.ToDictionary(i => i.Key, i => Candle.Create(i.Value));
        }

        public string DescribeState(IImmutableDictionary<string, ICandle> state)
        {
            return $"Candles count: {state.Count}";
        }

        private static string GetKey(string assetPairId, TimeInterval timeInterval, PriceType priceType)
        {
            return $"{assetPairId.Trim().ToUpper()}-{priceType}-{timeInterval}";
        }
    }
}
