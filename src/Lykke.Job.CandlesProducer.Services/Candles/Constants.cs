using System.Collections.Immutable;
using Lykke.Domain.Prices;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class Constants
    {
        public static readonly ImmutableArray<TimeInterval> PublishedIntervals = ImmutableArray.Create
        (
            TimeInterval.Sec,
            TimeInterval.Minute,
            TimeInterval.Min5,
            TimeInterval.Min15,
            TimeInterval.Min30,
            TimeInterval.Hour,
            TimeInterval.Hour4,
            TimeInterval.Hour6,
            TimeInterval.Hour12,
            TimeInterval.Day,
            TimeInterval.Week,
            TimeInterval.Month
        );
    }
}