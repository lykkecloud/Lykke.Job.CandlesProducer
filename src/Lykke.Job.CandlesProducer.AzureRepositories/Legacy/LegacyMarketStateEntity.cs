using System;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.AzureRepositories.Legacy
{
    [Obsolete("Used for snapshot migration")]
    public class LegacyMarketStateEntity : IMarketState
    {
        [JsonProperty("a")]
        public LegacyPriceStateEntity Ask { get; set; }
        [JsonProperty("b")]
        public LegacyPriceStateEntity Bid { get; set; }

        IPriceState IMarketState.Ask => Ask;
        IPriceState IMarketState.Bid => Bid;

        public static LegacyMarketStateEntity Create(IMarketState source)
        {
            return new LegacyMarketStateEntity
            {
                Ask = LegacyPriceStateEntity.Create(source.Ask),
                Bid = LegacyPriceStateEntity.Create(source.Bid)
            };
        }
    }
}
