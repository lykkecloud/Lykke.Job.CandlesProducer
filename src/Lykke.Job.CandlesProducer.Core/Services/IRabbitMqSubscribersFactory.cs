// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Common;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Core.Services
{
    public interface IRabbitMqSubscribersFactory
    {
        IStopable Create<TMessage>(RabbitMqSubscriptionSettings settings, Func<TMessage, Task> handler);
    }
}
