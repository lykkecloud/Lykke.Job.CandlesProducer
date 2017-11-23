using System.Collections.Generic;
using System.Collections.Immutable;
using Lykke.Domain.Prices;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class Constants
    {
        // Stores 1 day + 1 tick at least, to let quotes and trades meet each other in the candles cache
        public static readonly ImmutableDictionary<TimeInterval, int> PublishedIntervalsHistoryDepth =
            new Dictionary<TimeInterval, int>
            {
                {TimeInterval.Sec, 60 * 60 * 24 + 1},
                {TimeInterval.Minute, 60 * 24 + 1},
                {TimeInterval.Min5, 60 / 5 * 24 + 1},
                {TimeInterval.Min15, 60 / 15 * 24 + 1},
                {TimeInterval.Min30, 60 / 30 * 24 + 1},
                {TimeInterval.Hour, 24 + 1},
                {TimeInterval.Hour4, 24 / 4 + 1},
                {TimeInterval.Hour6, 24 / 6 + 1},
                {TimeInterval.Hour12, 24 / 12 + 1},
                {TimeInterval.Day, 2},
                {TimeInterval.Week, 2},
                {TimeInterval.Month, 2}
            }.ToImmutableDictionary();
    }
}
