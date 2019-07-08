// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Autofac;
using Common;

namespace Lykke.Job.CandlesProducer.Core.Services.Trades
{
    public interface ITradesSubscriber : IStartable, IStopable
    {
    }
}
