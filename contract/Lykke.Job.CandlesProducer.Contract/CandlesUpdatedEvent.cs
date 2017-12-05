using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.CandlesProducer.Contract
{
    /// <summary>
    /// Candle updates event
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class CandlesUpdatedEvent
    {
        /// <summary>
        /// Contract version
        /// </summary>
        [Key(0)]
        public Version ContractVersion { get; set; }

        /// <summary>
        /// Timestamp of the event publishing
        /// </summary>
        [Key(1)]
        public DateTime UpdateTimestamp { get; set; }

        /// <summary>
        /// Candle updates
        /// </summary>
        [Key(2)]
        public IReadOnlyList<CandleUpdate> Candles { get; set; }
    }
}
