using System;
using Lykke.Domain.Prices;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    // TODO: Move to the Contract assembly
    public class CandleMessage : ICandle
    {
        [JsonProperty("a")]
        public string AssetPairId { get; set; }

        [JsonProperty("p")]
        public PriceType PriceType { get; set; }

        [JsonProperty("i")]
        public TimeInterval TimeInterval { get; set; }

        [JsonProperty("t")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("tv")]
        public double TradingVolume { get; set; }

        [JsonProperty("u")]
        public DateTime LastUpdateTimestamp { get; set; }

        public static CandleMessage Copy(ICandle candle)
        {
            return new CandleMessage
            {
                AssetPairId = candle.AssetPairId,
                PriceType = candle.PriceType,
                TimeInterval = candle.TimeInterval,
                Timestamp = candle.Timestamp,
                Open = candle.Open,
                Close = candle.Close,
                Low = candle.Low,
                High = candle.High,
                TradingVolume = candle.TradingVolume,
                LastUpdateTimestamp = candle.LastUpdateTimestamp
            };
        }
    }
}
