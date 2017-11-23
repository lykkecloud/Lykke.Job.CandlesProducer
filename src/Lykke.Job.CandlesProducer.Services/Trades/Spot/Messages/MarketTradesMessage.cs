using System;
using JetBrains.Annotations;

namespace Lykke.Job.CandlesProducer.Services.Trades.Spot.Messages
{
    // TODO: Remove unsued fields

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class MarketTradesMessage
    {
        public MarketOrder Order { get; set; }
        public Trade[] Trades { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public class MarketOrder
        {
            public string Id { get; set; }
            public string ExternalId { get; set; }
            public string AssetPairId { get; set; }
            public string ClientId { get; set; }
            public double Volume { get; set; }
            public double? Price { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime Registered { get; set; }
            public DateTime? MatchedAt { get; set; }
            public bool Straight { get; set; }
            public double ReservedLimitVolume { get; set; }
            public double? DustSize { get; set; }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        public class Trade
        {
            public double Price { get; set; }
            public string LimitOrderId { get; set; }
            public string LimitOrderExternalId { get; set; }
            public DateTime Timestamp { get; set; }
            public string MarketClientId { get; set; }
            public string MarketAsset { get; set; }
            public double MarketVolume { get; set; }
            public string LimitClientId { get; set; }
            public double LimitVolume { get; set; }
            public string LimitAsset { get; set; }
        }
    }
}
