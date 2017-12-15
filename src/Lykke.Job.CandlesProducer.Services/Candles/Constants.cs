using System.Collections.Generic;
using System.Collections.Immutable;
using Lykke.Job.CandlesProducer.Contract;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public static class Constants
    {
        public static readonly HashSet<CandleTimeInterval> PublishedIntervals =
            new HashSet<CandleTimeInterval>
            {
                CandleTimeInterval.Sec,
                CandleTimeInterval.Minute,
                CandleTimeInterval.Min5,
                CandleTimeInterval.Min15,
                CandleTimeInterval.Min30,
                CandleTimeInterval.Hour,
                CandleTimeInterval.Hour4,
                CandleTimeInterval.Hour6,
                CandleTimeInterval.Hour12,
                CandleTimeInterval.Day,
                CandleTimeInterval.Week,
                CandleTimeInterval.Month
            };
    }
}
