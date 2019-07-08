// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace Lykke.Job.CandlesProducer.Core.Domain.Candles
{
    public interface IPriceState
    {
        double Price { get; }
        DateTime Moment { get; }
    }
}