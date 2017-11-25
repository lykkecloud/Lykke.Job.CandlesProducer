using System.Collections.Generic;
using System.Collections.Immutable;
using Lykke.Job.CandlesProducer.Contract;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public static class Constants
    {
        // Stores 1 day + 1 tick at least, to let quotes and trades meet each other in the candles cache
        public static readonly ImmutableDictionary<CandleTimeInterval, int> PublishedIntervalsHistoryDepth =
            new Dictionary<CandleTimeInterval, int>
            {
                {CandleTimeInterval.Sec, 60 * 60 * 24 + 1},
                {CandleTimeInterval.Minute, 60 * 24 + 1},
                {CandleTimeInterval.Min5, 60 / 5 * 24 + 1},
                {CandleTimeInterval.Min15, 60 / 15 * 24 + 1},
                {CandleTimeInterval.Min30, 60 / 30 * 24 + 1},
                {CandleTimeInterval.Hour, 24 + 1},
                {CandleTimeInterval.Hour4, 24 / 4 + 1},
                {CandleTimeInterval.Hour6, 24 / 6 + 1},
                {CandleTimeInterval.Hour12, 24 / 12 + 1},
                {CandleTimeInterval.Day, 2},
                {CandleTimeInterval.Week, 2},
                {CandleTimeInterval.Month, 2}
            }.ToImmutableDictionary();
    }
}
