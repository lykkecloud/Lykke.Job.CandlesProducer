using System;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.AzureRepositories.Legacy
{
    [Obsolete("Used for snapshot migration")]
    public class LegacyPriceStateEntity : IPriceState
    {
        [JsonProperty("p")]
        public double Price { get; set; }
        [JsonProperty("m")]
        public DateTime Moment { get; set; }

        public static LegacyPriceStateEntity Create(IPriceState source)
        {
            if (source == null)
            {
                return null;
            }

            return new LegacyPriceStateEntity
            {
                Price = source.Price,
                Moment = source.Moment
            };
        }
    }
}
