﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;

namespace Lykke.Job.CandlesProducer.Services.Trades.Spot.Messages
{
    // TODO: Remove unsued fields

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class LimitOrdersMessage
    {
        public LimitOrder[] Orders { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public class LimitOrder
        {
            public Order Order { get; set; }
            public Trade[] Trades { get; set; }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public class Order
        {
            public double? Price { get; set; }
            public double RemainingVolume { get; set; }
            public DateTime? LastMatchTime { get; set; }
            public string Id { get; set; }
            public string ExternalId { get; set; }
            public string AssetPairId { get; set; }
            public string ClientId { get; set; }
            public double Volume { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime Registered { get; set; }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public class Trade
        {
            public string ClientId { get; set; }
            public string Asset { get; set; }
            public double Volume { get; set; }
            public double Price { get; set; }
            public DateTime Timestamp { get; set; }
            public string OppositeOrderId { get; set; }
            public string OppositeOrderExternalId { get; set; }
            public string OppositeAsset { get; set; }
            public string OppositeClientId { get; set; }
            public double OppositeVolume { get; set; }
            public long Index { get; set; }
        }
    }
}
