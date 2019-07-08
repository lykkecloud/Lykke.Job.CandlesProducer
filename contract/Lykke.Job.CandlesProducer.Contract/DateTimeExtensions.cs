// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Common;
using JetBrains.Annotations;

namespace Lykke.Job.CandlesProducer.Contract
{
    [PublicAPI]
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Truncates the <paramref name="dateTime"/> to the precision of the given <paramref name="interval"/>
        /// </summary>
        public static DateTime TruncateTo(this DateTime dateTime, CandleTimeInterval interval)
        {
            switch (interval)
            {
                case CandleTimeInterval.Month:
                    return dateTime.RoundToMonth();
                case CandleTimeInterval.Week:
                    return dateTime.RoundToWeek();
                case CandleTimeInterval.Day:
                    return dateTime.Date;
                case CandleTimeInterval.Hour12:
                    return dateTime.RoundToHour(12);
                case CandleTimeInterval.Hour6:
                    return dateTime.RoundToHour(6);
                case CandleTimeInterval.Hour4:
                    return dateTime.RoundToHour(4);
                case CandleTimeInterval.Hour:
                    return dateTime.RoundToHour();
                case CandleTimeInterval.Min30:
                    return dateTime.RoundToMinute(30);
                case CandleTimeInterval.Min15:
                    return dateTime.RoundToMinute(15);
                case CandleTimeInterval.Min5:
                    return dateTime.RoundToMinute(5);
                case CandleTimeInterval.Minute:
                    return dateTime.RoundToMinute();
                case CandleTimeInterval.Sec:
                    return dateTime.RoundToSecond();
                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unexpected CandleTimeInterval value.");
            }
        }
    }
}
