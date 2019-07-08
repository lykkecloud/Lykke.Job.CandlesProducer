// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using MessagePack;

namespace Lykke.Job.CandlesProducer.AzureRepositories
{
    [MessagePackObject]
    public class MarketStateEntity : IMarketState
    {
        [Key(0)]
        public PriceStateEntity Ask { get; set; }
        [Key(1)]
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
