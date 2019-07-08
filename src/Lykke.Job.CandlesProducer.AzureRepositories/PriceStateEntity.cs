// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using MessagePack;

namespace Lykke.Job.CandlesProducer.AzureRepositories
{
    [MessagePackObject]
    public class PriceStateEntity : IPriceState
    {
        [Key(0)]
        public decimal Price { get; set; }
        [Key(1)]
        public DateTime Moment { get; set; }

        double IPriceState.Price => (double)Price;

        public static PriceStateEntity Create(IPriceState source)
        {
            if (source == null)
            {
                return null;
            }

            return new PriceStateEntity
            {
                Price = (decimal)source.Price,
                Moment = source.Moment
            };
        }
    }
}
