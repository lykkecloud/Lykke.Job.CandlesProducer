// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace Lykke.Job.CandlesProducer.Services.Trades.Mt.Messages
{
    // TODO: Remove unused fields

    public class MtTradeMessage
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string AccountId { get; set; }
        public string OrderId { get; set; }
        public string AssetPairId { get; set; }
        public TradeType Type { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }

        public enum TradeType
        {
            Buy,
            Sell
        }
    }
}
