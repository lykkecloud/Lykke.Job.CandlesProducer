using System;

namespace Lykke.Job.CandlesProducer.Core.Domain.Candles
{
    public interface IPriceState
    {
        double Price { get; }
        DateTime Moment { get; }
    }
}