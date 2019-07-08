﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace Lykke.Job.CandlesProducer.Contract
{
    [PublicAPI]
    public enum CandleTimeInterval
    {
        Unspecified = 0,
        Sec = 1,
        Minute = 60,
        Min5 = 300,
        Min15 = 900,
        Min30 = 1800,
        Hour = 3600,
        Hour4 = 7200,
        Hour6 = 21600,
        Hour12 = 43200,
        Day = 86400,
        Week = 604800,
        Month = 3000000
    }
}
