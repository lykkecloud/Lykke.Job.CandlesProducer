using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.AzureRepositories
{
    public class MarketStateEntity : IMarketState
    {
        [JsonProperty("a")]
        public PriceStateEntity Ask { get; set; }
        [JsonProperty("b")]
        public PriceStateEntity Bid { get; set; }

        IPriceState IMarketState.Ask => Ask;
        IPriceState IMarketState.Bid => Bid;

        public static MarketStateEntity Create(IMarketState source)
        {
            return new MarketStateEntity
            {
                Ask = PriceStateEntity.Create(source.Ask),
                Bid = PriceStateEntity.Create(source.Bid)
            };
        }
    }
}