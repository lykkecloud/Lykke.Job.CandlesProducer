// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Autofac;
using Common;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Core.Services.Trades
{
    public interface ITradesSubscriber : IStartable, IStopable
    {
        RabbitMqSubscriptionSettings SubscriptionSettings { get; }
    }
}
