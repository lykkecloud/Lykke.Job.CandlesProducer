// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace Lykke.Job.CandlesProducer.Core.Domain.Quotes
{
    public class MtQuoteDto
    {
        public string AssetPair { get; set; }

        public double Ask { get; set; }
        
        public double Bid { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
