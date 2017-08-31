using System;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.AzureRepositories
{
    public class PriceStateEntity : IPriceState
    {
        [JsonProperty("p")]
        public double Price { get; set; }
        [JsonProperty("m")]
        public DateTime Moment { get; set; }

        public static PriceStateEntity Create(IPriceState source)
        {
            if (source == null)
            {
                return null;
            }

            return new PriceStateEntity
            {
                Price = source.Price,
                Moment = source.Moment
            };
        }
    }
}