using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.Contract
{
    [PublicAPI]
    public class CandleMessage
    {
        [JsonProperty("a")]
        public string AssetPairId { get; set; }

        [JsonProperty("p")]
        public CandlePriceType PriceType { get; set; }

        [JsonProperty("i")]
        public CandleTimeInterval TimeInterval { get; set; }

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
    }
}
